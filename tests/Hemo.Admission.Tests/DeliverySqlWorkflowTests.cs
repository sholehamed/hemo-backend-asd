using System.Security.Claims;
using Application.Core.RoleAccess_core.Commands.RoleAccessCheckPagePermission;
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
public class DeliverySqlWorkflowTests
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
    public async Task NewAdmissionToCompletedTransfusionAndReactionAcrossRealHandlers()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_delivery_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new FixtureContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,true,true,true,true).Append(HemoDemoSeed.DashboardManager).ToArray();
            var config=accounts.ToDictionary(x=>"HemoSeed:Passwords:"+x.Key,x=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            config["HemoSeed:IncludeR02Pages"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(config).Build(),ct);
            var users=await db.UserRole.AsNoTracking().Include(x=>x.User).Include(x=>x.Role).ToListAsync(ct);
            Assert.Equal(15,users.Count);
            foreach(var account in accounts)
            {
                var assigned=users.Single(x=>x.User.Username==account.Username);
                Assert.True(assigned.Role.Gkey>2);
                var permissions=await db.RoleAccess.Where(x=>x.RoleGUID==assigned.RoleGUID).Select(x=>x.ApiGUID).ToListAsync(ct);
                Assert.Equal(account.Permissions.Length,permissions.Distinct().Count());
                Assert.DoesNotContain(permissions,p=>p==null);
            }
            void Actor(string username)
            {
                db.ChangeTracker.Clear();var role=users.Single(x=>x.User.Username==username);
                http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",role.UserGUID.ToString()),new Claim("UserRoles",role.Gkey.ToString())},"Fixture"));
                http.Request.Headers["UserRole"]=role.Gkey.ToString();
            }
            // Check the actual company page permission handler against the full activated menu set.
            var expectedPaths=new[]{
                "/admission",
                "/admission/dashboard",
                "/admission/detail/:guid",
                "/admission/discharge",
                "/admission/his",
                "/admission/history",
                "/admission/identity",
                "/admission/list",
                "/admission/new",
                "/admission/stats",
                "/hemo/bank",
                "/hemo/bank/crossmatch",
                "/hemo/bank/inventory",
                "/hemo/bank/lab-history",
                "/hemo/bank/receive",
                "/hemo/bank/reports",
                "/hemo/bank/temperature",
                "/hemo/bank/thaw",
                "/hemo/bank/unit/:guid",
                "/hemo/dashboard",
                "/hemo/hv",
                "/hemo/hv/bank",
                "/hemo/hv/capa",
                "/hemo/hv/cases",
                "/hemo/hv/detail/:guid",
                "/hemo/hv/doctor",
                "/hemo/hv/new",
                "/hemo/hv/rca",
                "/hemo/hv/reports",
                "/hemo/quality",
                "/hemo/quality/audit",
                "/hemo/quality/audit/:guid",
                "/hemo/quality/committee",
                "/hemo/quality/committee/:guid",
                "/hemo/quality/equipment",
                "/hemo/quality/equipment-dashboard",
                "/hemo/quality/equipment/:guid",
                "/hemo/quality/facility",
                "/hemo/quality/incidents",
                "/hemo/quality/maintenance",
                "/hemo/quality/qc",
                "/hemo/quality/qc/:guid",
                "/hemo/quality/temperature",
                "/hemo/quality/training",
                "/hemo/quality/training/:guid",
                "/hemo/requests",
                "/hemo/requests/approval",
                "/hemo/requests/bank",
                "/hemo/requests/bank/:guid",
                "/hemo/requests/detail/:guid",
                "/hemo/requests/doctor",
                "/hemo/requests/edit/:guid",
                "/hemo/requests/new",
                "/hemo/requests/orders",
                "/hemo/transfusion",
                "/hemo/transfusion/detail/:guid",
                "/hemo/transfusion/dispatch",
                "/hemo/transfusion/monitoring",
                "/hemo/transfusion/records",
                "/hemo/transfusion/returns",
                "/hemo/transfusion/tasks",
                "/hemo/transfusion/vitals"
            };
            var seededMenus=await db.Menu.AsNoTracking().Where(m=>expectedPaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(expectedPaths.OrderBy(x=>x),seededMenus.Select(m=>m.routerLink).OrderBy(x=>x));
            Assert.All(seededMenus,m=>Assert.False(m.IsPublic));
            var menuIds=seededMenus.Select(m=>m.GUID).ToArray();
            var grants=await db.RoleAccess.AsNoTracking().Where(a=>menuIds.Contains(a.MenuGUID)).ToListAsync(ct);
            Assert.All(seededMenus,m=>Assert.Contains(grants,g=>g.MenuGUID==m.GUID));
            Assert.All(grants,g=>Assert.NotNull(g.ApiGUID));
            var checkPage=new RoleAccessCheckPagePermissionCommandHandler(new RoleAccessService(db,null!),auth);
            var manager=users.Single(x=>x.User.Username=="hemo.dashboard.manager");
            foreach(var assignment in users)
            {
                Actor(assignment.User.Username);
                var permitted=grants.Where(g=>g.RoleGUID==assignment.RoleGUID).Select(g=>g.MenuGUID).ToHashSet();
                foreach(var m in seededMenus)
                    Assert.Equal(permitted.Contains(m.GUID),await checkPage.Handle(new RoleAccessCheckPagePermissionCommand{Menu=m.routerLink},ct));
                Assert.False(await checkPage.Handle(new RoleAccessCheckPagePermissionCommand{Menu="/hemo/not-a-route"},ct));
            }
            Actor("hemo.dashboard.manager");
            Assert.True(await checkPage.Handle(new RoleAccessCheckPagePermissionCommand{Menu="/hemo/dashboard"},ct));
            foreach(var p in new[]{"/admission/new","/admission/list","/admission/detail/:guid","/hemo/requests","/hemo/bank/inventory","/hemo/transfusion/tasks","/hemo/hv/cases","/hemo/quality/equipment/:guid","/hemo/quality/qc/:guid","/hemo/quality/facility"})
                Assert.False(await checkPage.Handle(new RoleAccessCheckPagePermissionCommand{Menu=p},ct));
            Assert.Equal(7,grants.Where(g=>g.RoleGUID==manager.RoleGUID).Select(g=>g.ApiGUID).Distinct().Count());
            var roles=new UserRoleService(db,null!,auth);
            var patients=new PatientService(db,null!);var admissions=new PatientAdmissionService(db,null!);var reviews=new PatientAdmissionReviewService(db,null!);
            var create=new CreatePatientAdmissionCommandHandler(patients,admissions,roles,auth);
            var aq=new PatientAdmissionQueryHandler(admissions,patients,reviews,roles,auth);
            var review=new ReviewAdmissionCommandHandler(admissions,reviews,roles,auth);
            var rs=new BloodRequestService(db,null!);var rq=new BloodRequestQueryHandler(rs,roles,auth);var rc=new BloodRequestCommandHandler(rs,roles,auth);
            var bank=new BankCommandHandler(new BankBloodUnitService(db,null!),roles,auth);var bq=new BankQueryHandler(new BankBloodUnitService(db,null!),roles,auth);
            var es=new HemoEquipmentService(db,null!);var ec=new EquipmentCommandHandler(es,roles,auth);var eq=new EquipmentQueryHandler(es,roles,auth);
            var ts=new BloodTransferService(db,null!);var tx=new TransferCommandHandler(ts,roles,auth);var tq=new TransferQueryHandler(ts,roles,auth);
            var hs=new HemoReactionService(db,null!);var hv=new HvCommandHandler(hs,roles,auth);var hq=new HvQueryHandler(hs,roles,auth);

            Actor("hemo.admission");
            var admission=await create.Handle(new CreatePatientAdmissionCommand{IdentificationType="Unknown",FirstName="Synthetic",LastName="Delivery",HisNumber="FIX-ST08-01",Ward="Fixture",BedNumber="1",Physician="Fixture"},ct);
            var a=await aq.Handle(new PatientAdmissionFindByIdQuery(admission),ct);
            await review.Handle(new ReviewAdmissionCommand(admission,"Identity","Verified",31,null,a.Version),ct);
            db.ChangeTracker.Clear();a=await aq.Handle(new PatientAdmissionFindByIdQuery(admission),ct);
            await review.Handle(new ReviewAdmissionCommand(admission,"History","Reviewed",0,"Synthetic history",a.Version),ct);
            Actor("hemo.nurse");
            var request=await rc.Handle(new CreateNurseRequestCommand(new(admission,Guid.NewGuid(),"General","Routine",new(){new("RBC",1)},new(ClinicalIndication:"Synthetic fixture only"))),ct);
            var d=await rq.Handle(new BloodRequestDetailQuery(request),ct);
            await rc.Handle(new SubmitBloodRequestCommand(new(request,d.Version,"Submit")),ct);
            Actor("hemo.admission");a=await aq.Handle(new PatientAdmissionFindByIdQuery(admission),ct);
            await Assert.ThrowsAsync<ServiceExeption>(()=>review.Handle(new ReviewAdmissionCommand(admission,"Clearance","Ready",31,"Fixture",a.Version),ct));
            Actor("hemo.doctor");d=await rq.Handle(new BloodRequestDetailQuery(request),ct);
            await rc.Handle(new DecideBloodRequestCommand(new(request,d.Version,"Approve",null,new("Fixture","Fixture","FIX-ONLY",null,true))),ct);

            Actor("hemo.equipment.tech");
            var equipment=await ec.Handle(new CreateEquipmentCommand(new("FIX-ST08-FRIDGE","Synthetic fridge","Refrigerator","Fixture model","Fixture ward")),ct);
            db.ChangeTracker.Clear();var e=await eq.Handle(new EquipmentDetailQuery(equipment),ct);
            await ec.Handle(new EquipmentServiceCommand(new(equipment,e.Equipment.Version,"Calibration",DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddDays(1),"FIX","Synthetic provider","Accepted","Synthetic checks","Synthetic only")),ct);
            Actor("hemo.equipment.reviewer");e=await eq.Handle(new EquipmentDetailQuery(equipment),ct);
            await ec.Handle(new EquipmentStateCommand(new(equipment,e.Equipment.Version,"Synthetic independent review","FIX"),"Release"),ct);
            Actor("hemo.equipment.tech");e=await eq.Handle(new EquipmentDetailQuery(equipment),ct);
            await ec.Handle(new EquipmentTemperatureCommand(new(equipment,e.Equipment.Version,DateTimeOffset.UtcNow,"Morning",0m,-1m,1m,true,true,true,"Acceptable","Synthetic observation")),ct);
            Actor("hemo.bloodbank");
            var now=DateTimeOffset.UtcNow;
            var unit=await bank.Handle(new ReceiveBankUnitCommand(new("FIX-ST08-U1","RBC","O+",null,"Synthetic supplier","FIX-ST08-FRIDGE",now.AddMinutes(-1),now.AddDays(1))),ct);
            async Task<UnitDetail> Unit(){db.ChangeTracker.Clear();return await bq.Handle(new BankUnitQuery(unit),ct);}
            async Task BankAction(string action,Guid? req=null,byte[]? version=null)
            {
                var snapshot=await Unit();await bank.Handle(new BankUnitActionCommand(new(unit,snapshot.Unit.Version,action,req,version,"Synthetic action"),action),ct);
            }
            Actor("hemo.bank.supervisor");await BankAction("Inspect");
            Actor("hemo.bloodbank");d=await rq.Handle(new BloodRequestBankDetailQuery(request),ct);var rv=d.Version;
            var bankSnapshot=await Unit();
            await bank.Handle(new RecordBankLabCommand(new(unit,bankSnapshot.Unit.Version,request,rv,"FIX","Confirmed","O+","Synthetic method","NonReactive","NonReactive","NonReactive","NonReactive","NonReactive","FIX-REPORT","Accepted","Synthetic fixture")),ct);
            await BankAction("Reserve",request,rv);await BankAction("Ready");
            Actor("hemo.bank.supervisor");
            var candidate=Assert.Single((await tq.Handle(new DispatchCandidatesQuery(),ct)).Data);
            Assert.Equal(unit,candidate.UnitGuid);
            var transfer=await tx.Handle(new DispatchTransferCommand(new(candidate.UnitGuid,candidate.UnitVersion,candidate.UnitCode,candidate.WristbandCode,"Synthetic courier","FIX","FIX-BOX",0m,"Synthetic indicator",null,31,"Synthetic fixture")),ct);
            async Task<TransferDetail> Transfer(){db.ChangeTracker.Clear();return await tq.Handle(new TransferDetailQuery(transfer),ct);}
            async Task TransferAction(string action,Func<TransferActionInput,TransferActionInput>? edit=null)
            {
                var t=await Transfer();var v=new TransferActionInput(transfer,t.Transfer.Version,t.Transfer.UnitCode,t.WristbandCode,31,"Synthetic fixture",0m,"Acceptable","Synthetic receiver","Synthetic equipment");
                await tx.Handle(new TransferActionCommand(edit==null?v:edit(v),action),ct);
            }
            async Task Vital(string phase)
            {
                var t=await Transfer();await tx.Handle(new RecordTransferVitalCommand(new(transfer,t.Transfer.Version,phase,DateTimeOffset.UtcNow,120,80,75,37m,98,16,null,"Synthetic observation",10m)),ct);
            }
            Actor("hemo.nurse");await TransferAction("Receive");await TransferAction("Verify");
            await Assert.ThrowsAsync<ServiceExeption>(()=>TransferAction("Witness"));
            Actor("hemo.nurse.witness");await TransferAction("Witness");
            Actor("hemo.nurse");await Vital("T0");await TransferAction("Start");await Vital("T15");await Vital("End");
            await TransferAction("Complete",x=>x with{VolumeMl=100m});
            var done=await Transfer();Assert.Equal("Completed",done.Transfer.Status);Assert.NotEqual(done.VerifiedBy,done.WitnessedBy);
            // A later documented reaction is linked to the actual completed transfer.
            var reaction=await hv.Handle(new ReportReactionCommand(new(transfer,done.Transfer.Version,Guid.NewGuid(),DateTimeOffset.UtcNow,new[]{"Unknown"},"Unknown","Synthetic observation","Synthetic record")),ct);
            async Task HvReview(string action,Func<ReviewInput,ReviewInput>? edit=null)
            {
                db.ChangeTracker.Clear();var detail=await hq.Handle(new HvDetailQuery(reaction),ct);
                var v=new ReviewInput(reaction,detail.Case.Version,"Synthetic fixture");await hv.Handle(new ReviewReactionCommand(edit==null?v:edit(v),action),ct);
            }
            Actor("hemo.doctor");await HvReview("DoctorReview",x=>x with{DiagnosisCode="Undetermined",Severity="Unknown",Imputability="Unknown",Outcome="Unknown",Actions="Synthetic action",DoctorReference="FIX"});
            Actor("hemo.bloodbank");await HvReview("BankReview",x=>x with{SampleReference="FIX",ReportReference="FIX",Disposition="Synthetic disposition"});
            Actor("hemo.hv.officer");await HvReview("Investigate",x=>x with{RcaRequired=false,ExternalStatus="NotRequired"});
            Actor("hemo.hv.reviewer");await HvReview("Close");
            db.ChangeTracker.Clear();Assert.Equal("Closed",(await hq.Handle(new HvDetailQuery(reaction),ct)).Case.Status);
            Assert.Equal(transfer,await db.HemoReaction.Where(x=>x.GUID==reaction).Select(x=>x.BloodTransferGUID).SingleAsync(ct));
            Assert.Equal(admission,await db.BloodTransfer.Where(x=>x.GUID==transfer).Select(x=>x.PatientAdmissionGUID).SingleAsync(ct));
            Assert.Equal(request,await db.BloodTransfer.Where(x=>x.GUID==transfer).Select(x=>x.BloodRequestGUID).SingleAsync(ct));
            Assert.Equal("Transfused",await db.BankBloodUnit.Where(x=>x.GUID==unit).Select(x=>x.Status).SingleAsync(ct));
            Actor("hemo.admission");a=await aq.Handle(new PatientAdmissionFindByIdQuery(admission),ct);
            await review.Handle(new ReviewAdmissionCommand(admission,"Clearance","Ready",31,"Synthetic documented review",a.Version),ct);
            db.ChangeTracker.Clear();Assert.Equal("ReadyForDischarge",(await aq.Handle(new PatientAdmissionFindByIdQuery(admission),ct)).Status);
            Actor("hemo.nurse");await Assert.ThrowsAsync<ServiceExeption>(()=>rc.Handle(new CreateNurseRequestCommand(new(admission,Guid.NewGuid(),"General","Routine",new(){new("RBC",1)},new(ClinicalIndication:"Synthetic fixture"))),ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }
}
