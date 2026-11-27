using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.BloodRequest_core;
using Application.Core.BloodBank_core;
using Application.Core.Transfusion_core;
using Application.Core.Hemovigilance_core;
using Application.Core.Quality_core;
using Application.Core.Quality_core.Commands;
using Application.Core.Quality_core.Queries;
using Application.Core.Hemovigilance_core.Commands;
using Application.Core.Hemovigilance_core.Queries;
using Application.Core.Transfusion_core.Commands;
using Application.Core.Transfusion_core.Queries;
using Application.Core.BloodBank_core.Commands;
using Application.Core.BloodBank_core.Queries;
using Application.Core.BloodRequest_core.Commands;
using Application.Core.BloodRequest_core.Queries;
using Application.Core.PatientAdmission_core.Commands.CreatePatientAdmission;
using Application.Core.PatientAdmission_core.Commands.ReviewAdmission;
using Application.Core.PatientAdmission_core.Queries;
using Application.Repository.Services;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hemo.Admission.Tests;
public class QualitySqlWorkflowTests
{
    // Each test actor needs its own accessor; HttpContextAccessor uses a shared AsyncLocal holder.
    private sealed class FixtureContextAccessor : IHttpContextAccessor { public HttpContext? HttpContext { get; set; } }
    private sealed class FixtureAudit(AuthService auth):SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e,InterceptionResult<int> result,CancellationToken ct=default)
        {
            foreach(var row in e.Context!.ChangeTracker.Entries<BaseEntity>())
            {
                if(row.State==EntityState.Added)row.Entity.Created=DateTime.UtcNow;
                if(row.Entity is AuditedEntity a && row.State is EntityState.Added or EntityState.Modified)
                {
                    if(row.State==EntityState.Added){a.CreatedBy=auth.GetUserGUID();a.CreatedByIP="test";}
                    a.LastModified=DateTime.UtcNow;a.LastModifiedBy=auth.GetUserGUID();a.LastModifiedByIP="test";
                }
            }
            return ValueTask.FromResult(result);
        }
    }
    [Fact]
    [Trait("Category","SqlServer")]
    public async Task EquipmentAndQualityWorkflowsAreScopedAndRepeatable()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST07_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new FixtureContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var secrets=HemoDemoSeed.DemoAccounts(true,true,true,true,true).ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeQualitySamples"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(secrets).Build(),ct);
            var users=await db.UserRole.AsNoTracking().Include(x=>x.User).Include(x=>x.Role).ToListAsync(ct);
            void Actor(string name)
            {
                db.ChangeTracker.Clear();var role=users.Single(x=>x.User.Username==name);
                http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",role.UserGUID.ToString()),new Claim("UserRoles",role.Gkey.ToString())},"Fixture"));
                http.Request.Headers["UserRole"]=role.Gkey.ToString();
            }
            var roles=new UserRoleService(db,null!,auth);var requests=new BloodRequestService(db,null!);
            var patients=new PatientService(db,null!);var admissions=new PatientAdmissionService(db,null!);var reviews=new PatientAdmissionReviewService(db,null!);
            var admissionCreate=new CreatePatientAdmissionCommandHandler(patients,admissions,roles,auth);
            var admissionQuery=new PatientAdmissionQueryHandler(admissions,patients,reviews,roles,auth);
            var review=new ReviewAdmissionCommandHandler(admissions,reviews,roles,auth);
            var commands=new BloodRequestCommandHandler(requests,roles,auth);var query=new BloodRequestQueryHandler(requests,roles,auth);
            var es=new HemoEquipmentService(db,null!);var ec=new EquipmentCommandHandler(es,roles,auth);var eq=new EquipmentQueryHandler(es,roles,auth);
            var qs=new HemoQualityRecordService(db,null!);var qc=new QualityCommandHandler(qs,roles,auth);var qq=new QualityQueryHandler(qs,roles,auth);
            Assert.Equal(4,await db.HemoEquipment.CountAsync(ct));Assert.Equal(4,await db.HemoQualityRecord.CountAsync(ct));Assert.Equal(4,await db.HemoQualityLine.CountAsync(ct));
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HemoSeed:IncludeQualitySamples","true"}}).Build(),ct);
            Assert.Equal(4,await db.HemoEquipment.CountAsync(ct));Assert.Equal(4,await db.HemoQualityRecord.CountAsync(ct));
            Actor("hemo.viewer");Assert.False((await qq.Handle(new QualityCapabilitiesQuery(),ct)).Permissions["equipmentList"]);await Assert.ThrowsAsync<ForbiddenAccessException>(()=>eq.Handle(new EquipmentListQuery(new()),ct));
            Actor("hemo.equipment.tech");var cap=await qq.Handle(new QualityCapabilitiesQuery(),ct);Assert.True(cap.Permissions["equipmentCreate"]);Assert.False(cap.Permissions["equipmentRelease"]);
            var list=await eq.Handle(new EquipmentListQuery(new()),ct);Assert.Equal(4,list.TotalCount);
            var fridge=list.Data.Single(x=>x.Code=="DEMO-ST07-FRIDGE");var centrifuge=list.Data.Single(x=>x.Code=="DEMO-ST07-CENTRIFUGE");var freezer=list.Data.Single(x=>x.Code=="DEMO-ST07-FREEZER");
            var org=users.Single(x=>x.User.Username=="hemo.equipment.tech").OrgLocationGUID;
            Assert.True(await EquipmentInterlock.Available(db,org,fridge.Code,ct));Assert.False(await EquipmentInterlock.Available(db,org,freezer.Code,ct));Assert.True(await EquipmentInterlock.Available(db,org,"LEGACY",ct));
            async Task<EquipmentDetail> Equipment(Guid id){db.ChangeTracker.Clear();return await eq.Handle(new EquipmentDetailQuery(id),ct);}
            var cd=await Equipment(centrifuge.Guid);var i=Assert.Single(cd.Incidents);
            await ec.Handle(new ResolveEquipmentIncidentCommand(new(i.Guid,i.Version,cd.Equipment.Version,"Synthetic repair","DEMO")),ct);
            Actor("hemo.equipment.reviewer");cd=await Equipment(centrifuge.Guid);await Assert.ThrowsAsync<ServiceExeption>(()=>ec.Handle(new EquipmentStateCommand(new(cd.Equipment.Guid,cd.Equipment.Version,"Fixture review","DEMO"),"Release"),ct));
            Actor("hemo.equipment.tech");cd=await Equipment(centrifuge.Guid);await ec.Handle(new EquipmentServiceCommand(new(cd.Equipment.Guid,cd.Equipment.Version,"Calibration",DateTimeOffset.UtcNow.AddHours(-1),DateTimeOffset.UtcNow.AddDays(1),"DEMO","Synthetic provider","Accepted","Synthetic checks","Old performed date")),ct);
            Actor("hemo.equipment.reviewer");cd=await Equipment(centrifuge.Guid);await Assert.ThrowsAsync<ServiceExeption>(()=>ec.Handle(new EquipmentStateCommand(new(cd.Equipment.Guid,cd.Equipment.Version,"Fixture review","DEMO"),"Release"),ct));
            Actor("hemo.equipment.tech");cd=await Equipment(centrifuge.Guid);await ec.Handle(new EquipmentServiceCommand(new(cd.Equipment.Guid,cd.Equipment.Version,"Calibration",DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddDays(2),"DEMO","Synthetic provider","Accepted","Synthetic checks","Fixture service")),ct);
            Actor("hemo.equipment.reviewer");cd=await Equipment(centrifuge.Guid);await ec.Handle(new EquipmentStateCommand(new(cd.Equipment.Guid,cd.Equipment.Version,"Fixture independent review","DEMO"),"Release"),ct);
            Assert.Equal("Operational",(await Equipment(centrifuge.Guid)).Equipment.Status);
            Actor("hemo.equipment.tech");var fd=await Equipment(fridge.Guid);
            var bankUnit=await db.BankBloodUnit.AsNoTracking().SingleAsync(x=>x.OrgLocationGUID==org&&x.UnitCode=="DEMO-ST07-U1",ct);Assert.Equal(1,fd.StoredUnits);
            await ec.Handle(new EquipmentTemperatureCommand(new(fridge.Guid,fd.Equipment.Version,DateTimeOffset.UtcNow,"Morning",0m,-1m,1m,true,true,true,"Concern","Synthetic concern")),ct);
            fd=await Equipment(fridge.Guid);Assert.Equal("Hold",fd.Equipment.Status);Assert.Equal(2,fd.Temperatures.Count);Assert.Equal("Quarantined",(await db.BankBloodUnit.AsNoTracking().SingleAsync(x=>x.GUID==bankUnit.GUID,ct)).Status);
            Assert.False(await EquipmentInterlock.Available(db,org,fridge.Code,ct));
            Actor("hemo.bloodbank");var bhandler=new BankCommandHandler(new BankBloodUnitService(db,null!),roles,auth);await Assert.ThrowsAsync<ServiceExeption>(()=>bhandler.Handle(new ReceiveBankUnitCommand(new("DEMO-BLOCKED","RBC","O+",null,"Fixture",fridge.Code,DateTimeOffset.UtcNow.AddMinutes(-1),DateTimeOffset.UtcNow.AddDays(1))),ct));
            var bquery=new BankQueryHandler(new BankBloodUnitService(db,null!),roles,auth);Assert.True((await bquery.Handle(new BankUnitQuery(bankUnit.GUID),ct)).Unit.EquipmentBlocked);
            Actor("hemo.bank.supervisor");Assert.Equal(1,(await bquery.Handle(new BankStatsQuery(),ct)).EquipmentBlocked);
            Actor("hemo.equipment.reviewer");await Assert.ThrowsAsync<ServiceExeption>(()=>ec.Handle(new EquipmentStateCommand(new(fridge.Guid,fd.Equipment.Version,"Fixture"),"Archive"),ct));
            Actor("hemo.quality.officer");var qcap=await qq.Handle(new QualityCapabilitiesQuery(),ct);Assert.True(qcap.Permissions["auditCreate"]);Assert.False(qcap.Permissions["auditApprove"]);
            async Task<QualityDetail> Get(string kind,Guid id){db.ChangeTracker.Clear();return await qq.Handle(new QualityDetailQuery(kind,id),ct);}
            async Task Review(string kind,Guid id,string action){var d=await Get(kind,id);await qc.Handle(new QualityReviewCommand(kind,action,new(id,d.Record.Version,"Fixture review")),ct);}
            var audit=Assert.Single((await qq.Handle(new QualityListQuery("Audit",new()),ct)).Data);var ad=await Get("Audit",audit.Guid);Assert.Equal(1,ad.AuditScore);Assert.Equal(2,ad.AuditMaximum);
            await qc.Handle(new SaveQualityLineCommand("Audit",new(audit.Guid,ad.Record.Version,"Fixture clause",new QualityLineData(Clause:"DEMO-2",Score:2))),ct);
            await Review("Audit",audit.Guid,"Submit");
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>Review("Audit",audit.Guid,"Approve"));
            Actor("hemo.quality.reviewer");await Review("Audit",audit.Guid,"Approve");Assert.Equal("Approved",(await Get("Audit",audit.Guid)).Record.Status);
            var qrow=Assert.Single((await qq.Handle(new QualityListQuery("Qc",new()),ct)).Data);await Review("Qc",qrow.Guid,"Approve");
            Assert.Equal("Approved",(await Get("Qc",qrow.Guid)).Record.Status);
            Actor("hemo.quality.officer");var tr=Assert.Single((await qq.Handle(new QualityListQuery("Training",new()),ct)).Data);var td=await Get("Training",tr.Guid);var line=Assert.Single(td.Lines);
            await Assert.ThrowsAsync<ServiceExeption>(()=>Review("Training",tr.Guid,"Submit"));
            td=await Get("Training",tr.Guid);line=Assert.Single(td.Lines);
            var owners=await qq.Handle(new QualityOwnersQuery("Training"),ct);Assert.Contains(owners,x=>x.UserGuid==line.OwnerUserGuid);
            await qc.Handle(new SaveQualityLineCommand("Training",new(tr.Guid,td.Record.Version,line.Title,line.Data with{Attendance="Present",PreScore=10,PostScore=15,CertificateReference="DEMO"},line.OwnerUserGuid,null,line.Guid,line.Version)),ct);
            await Review("Training",tr.Guid,"Submit");Actor("hemo.quality.reviewer");await Review("Training",tr.Guid,"Approve");
            Actor("hemo.nurse");var meeting=Assert.Single((await qq.Handle(new QualityListQuery("Committee",new(Mine:true)),ct)).Data);
            async Task Resolution(string action,int progress){var d=await Get("Committee",meeting.Guid);var l=Assert.Single(d.Lines);await qc.Handle(new ResolutionCommand(action,new(meeting.Guid,d.Record.Version,l.Guid,l.Version,progress,"Fixture evidence")),ct);}
            await Resolution("Progress",100);await Assert.ThrowsAsync<ForbiddenAccessException>(()=>Resolution("Verify",100));
            Actor("hemo.quality.reviewer");await Assert.ThrowsAsync<ServiceExeption>(()=>Review("Committee",meeting.Guid,"Close"));await Resolution("Return",100);
            Actor("hemo.nurse");await Resolution("Progress",100);Actor("hemo.quality.reviewer");await Resolution("Verify",100);await Review("Committee",meeting.Guid,"Close");
            Assert.Equal("Closed",(await Get("Committee",meeting.Guid)).Record.Status);
            var stats=await qq.Handle(new QualityStatsQuery(new()),ct);Assert.Equal(1,stats.Present);Assert.Equal(0,stats.OpenResolutions);Assert.Equal(1,stats.QcRecords);Assert.Equal(1,stats.Audits);
            Actor("hemo.quality.officer");var p=await qq.Handle(new FacilityQuery(),ct);await qc.Handle(new SaveFacilityCommand(new(p.Version,"Synthetic updated",null,null,null,null,null,null,"Fixture","Fixture change")),ct);db.ChangeTracker.Clear();var updated=await qq.Handle(new FacilityQuery(),ct);Assert.Equal("Synthetic updated",updated.FacilityName);Assert.Equal("Fixture change",Assert.Single(updated.History!).Notes);
            var actor=users.Single(x=>x.User.Username=="hemo.quality.reviewer");var other=new OrgLocation{Name="Other center"};db.OrgLocation.Add(other);await db.SaveChangesAsync(ct);
            var assignment=new UserRole{UserGUID=actor.UserGUID,RoleGUID=actor.RoleGUID,OrgLocationGUID=other.GUID,ExpireDate=DateTime.Now.AddDays(1)};db.UserRole.Add(assignment);await db.SaveChangesAsync(ct);db.ChangeTracker.Clear();
            http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",actor.UserGUID.ToString()),new Claim("UserRoles",assignment.Gkey.ToString())},"Fixture"));http.Request.Headers["UserRole"]=assignment.Gkey.ToString();
            Assert.Empty((await qq.Handle(new QualityListQuery("Audit",new()),ct)).Data);await Assert.ThrowsAsync<ServiceExeption>(()=>qq.Handle(new QualityDetailQuery("Audit",audit.Guid),ct));Assert.Equal(0,(await qq.Handle(new QualityStatsQuery(new()),ct)).Audits);
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task DeliverySeedIsRepeatableAndClearanceRejectsUnfinishedOrders()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST08_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new FixtureContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var secrets=HemoDemoSeed.DemoAccounts(true,true,true,true,true).ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeDeliverySamples"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(secrets).Build(),ct);
            var users=await db.UserRole.AsNoTracking().Include(x=>x.User).ToListAsync(ct);
            Assert.Equal(14,users.Count);
            var org=users.Single(x=>x.User.Username=="hemo.admission").OrgLocationGUID;
            Assert.Equal(8,await db.PatientAdmission.CountAsync(x=>x.OrgLocationGUID==org&&(x.HISNumber.StartsWith("DEMO-ST05-")||x.HISNumber.StartsWith("DEMO-ST06-")),ct));
            Assert.Equal(4,await db.HemoEquipment.CountAsync(ct));
            Assert.Equal(4,await db.HemoQualityRecord.CountAsync(ct));
            Assert.Equal(3,await db.HemoReaction.CountAsync(ct));
            var before=await db.BankBloodUnit.AsNoTracking().ToDictionaryAsync(x=>x.GUID,x=>x.ExpiresAt,ct);
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HemoSeed:IncludeDeliverySamples","true"}}).Build(),ct);
            Assert.Equal(14,await db.UserRole.CountAsync(ct));
            Assert.Equal(before.Count,await db.BankBloodUnit.CountAsync(ct));
            foreach(var u in await db.BankBloodUnit.AsNoTracking().ToListAsync(ct))Assert.Equal(before[u.GUID],u.ExpiresAt);
            Assert.Equal(3,await db.HemoReaction.CountAsync(ct));
            var actor=users.Single(x=>x.User.Username=="hemo.admission");
            db.ChangeTracker.Clear();
            http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",actor.UserGUID.ToString()),new Claim("UserRoles",actor.Gkey.ToString())},"Fixture"));
            http.Request.Headers["UserRole"]=actor.Gkey.ToString();
            var roles=new UserRoleService(db,null!,auth);
            var review=new ReviewAdmissionCommandHandler(new PatientAdmissionService(db,null!),new PatientAdmissionReviewService(db,null!),roles,auth);
            var a=await db.PatientAdmission.AsNoTracking().SingleAsync(x=>x.HISNumber=="DEMO-ST05-01"&&x.OrgLocationGUID==org,ct);
            await review.Handle(new ReviewAdmissionCommand(a.GUID,"History","Reviewed",0,"Synthetic history",a.Version),ct);
            db.ChangeTracker.Clear();a=await db.PatientAdmission.AsNoTracking().SingleAsync(x=>x.GUID==a.GUID,ct);
            var count=await db.PatientAdmissionReview.CountAsync(x=>x.PatientAdmissionGUID==a.GUID,ct);
            await Assert.ThrowsAsync<ServiceExeption>(()=>review.Handle(new ReviewAdmissionCommand(a.GUID,"Clearance","Ready",31,"Fixture",a.Version),ct));
            db.ChangeTracker.Clear();
            Assert.Equal("Active",(await db.PatientAdmission.AsNoTracking().SingleAsync(x=>x.GUID==a.GUID,ct)).Status);
            Assert.Equal(count,await db.PatientAdmissionReview.CountAsync(x=>x.PatientAdmissionGUID==a.GUID,ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task DashboardSeedHasScopedAggregateOnlyRoleAndHistoricalAdmissions()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST09_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new FixtureContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var secrets=HemoDemoSeed.DemoAccounts(true,true,true,true,true).Append(HemoDemoSeed.DashboardManager).ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeDashboardSamples"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(secrets).Build(),ct);
            var manager=await db.UserRole.AsNoTracking().Include(x=>x.User).SingleAsync(x=>x.User.Username=="hemo.dashboard.manager",ct);
            Assert.Equal(15,await db.UserRole.CountAsync(ct));
            Assert.Equal(7,await db.RoleAccess.CountAsync(x=>x.RoleGUID==manager.RoleGUID,ct));
            var sample=await db.PatientAdmission.AsNoTracking().Where(x=>x.OrgLocationGUID==manager.OrgLocationGUID&&x.HISNumber.StartsWith("DEMO-ST09-")).ToDictionaryAsync(x=>x.GUID,x=>x.AdmittedAt,ct);
            Assert.Equal(10,sample.Count);
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HemoSeed:IncludeDashboardSamples","true"}}).Build(),ct);
            Assert.Equal(15,await db.UserRole.CountAsync(ct));Assert.Equal(7,await db.RoleAccess.CountAsync(x=>x.RoleGUID==manager.RoleGUID,ct));
            var after=await db.PatientAdmission.AsNoTracking().Where(x=>x.OrgLocationGUID==manager.OrgLocationGUID&&x.HISNumber.StartsWith("DEMO-ST09-")).ToListAsync(ct);
            Assert.Equal(10,after.Count);foreach(var row in after){Assert.Equal(sample[row.GUID],row.AdmittedAt);Assert.Equal("Pending",row.IdentityStatus);}
            db.ChangeTracker.Clear();
            http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",manager.UserGUID.ToString()),new Claim("UserRoles",manager.Gkey.ToString())},"Fixture"));
            http.Request.Headers["UserRole"]=manager.Gkey.ToString();
            var roles=new UserRoleService(db,null!,auth);
            var aq=new PatientAdmissionQueryHandler(new PatientAdmissionService(db,null!),new PatientService(db,null!),new PatientAdmissionReviewService(db,null!),roles,auth);
            // Anchor to the actual fixture day, robust across a UTC midnight during CI.
            var day=sample.Values.Max().Date.AddDays(1);
            Assert.Equal(4,(await aq.Handle(new PatientAdmissionStatsQuery(day.AddDays(-7),day.AddDays(-1),"بخش آموزشی آمار داخلی"),ct)).Total);
            Assert.Equal(1,(await aq.Handle(new PatientAdmissionStatsQuery(day.AddDays(-14),day.AddDays(-8),"بخش آموزشی آمار داخلی"),ct)).Total);
            Assert.True((await aq.Handle(new PatientAdmissionStatsQuery(),ct)).Total>=18);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>aq.Handle(new PatientAdmissionWithPaginationQuery(),ct));
            var rq=new BloodRequestQueryHandler(new BloodRequestService(db,null!),roles,auth);
            Assert.True((await rq.Handle(new BloodRequestStatsQuery(),ct)).Total>0);
            var bq=new BankQueryHandler(new BankBloodUnitService(db,null!),roles,auth);
            Assert.True((await bq.Handle(new BankStatsQuery(),ct)).Total>0);
            var tq=new TransferQueryHandler(new BloodTransferService(db,null!),roles,auth);
            Assert.True((await tq.Handle(new TransferStatsQuery(),ct)).Total>0);
            var hq=new HvQueryHandler(new HemoReactionService(db,null!),roles,auth);
            Assert.Equal(3,(await hq.Handle(new HvStatsQuery(new()),ct)).Total);
            var eq=new EquipmentQueryHandler(new HemoEquipmentService(db,null!),roles,auth);
            Assert.Equal(4,(await eq.Handle(new EquipmentSummaryQuery(),ct)).Total);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>eq.Handle(new EquipmentListQuery(new()),ct));
            var qq=new QualityQueryHandler(new HemoQualityRecordService(db,null!),roles,auth);
            Assert.True((await qq.Handle(new QualityStatsQuery(new()),ct)).QcRecords>0);
            var caps=await qq.Handle(new QualityCapabilitiesQuery(),ct);
            Assert.True(caps.Permissions["stats"]);Assert.False(caps.Permissions["qcList"]);Assert.False(caps.Permissions["qcCreate"]);
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }
}
