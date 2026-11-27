using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.BloodRequest_core;
using Application.Core.BloodBank_core;
using Application.Core.Transfusion_core;
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
public class TransfusionSqlWorkflowTests
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
    public async Task SeededCasesExecuteDispatchWitnessVitalsCompletionAndReturn()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST05_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new FixtureContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var secrets=HemoDemoSeed.DemoAccounts(true,true,true).ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeTransfusionSamples"]="true";
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
            var service=new BloodTransferService(db,null!);var tx=new TransferCommandHandler(service,roles,auth);var tq=new TransferQueryHandler(service,roles,auth);
            Assert.Equal(4,await db.PatientAdmission.CountAsync(x=>x.HISNumber.StartsWith("DEMO-ST05-"),ct));
            Assert.Equal(4,await db.BankBloodUnit.CountAsync(x=>x.UnitCode.StartsWith("DEMO-UNIT-"),ct));
            var expiry=await db.BankBloodUnit.AsNoTracking().Where(x=>x.UnitCode=="DEMO-UNIT-01").Select(x=>x.ExpiresAt).SingleAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HemoSeed:IncludeTransfusionSamples","true"}}).Build(),ct);
            Assert.Equal(4,await db.BankBloodUnit.CountAsync(x=>x.UnitCode.StartsWith("DEMO-UNIT-"),ct));
            Assert.Equal(expiry,await db.BankBloodUnit.AsNoTracking().Where(x=>x.UnitCode=="DEMO-UNIT-01").Select(x=>x.ExpiresAt).SingleAsync(ct));
            Actor("hemo.viewer");Assert.False((await tq.Handle(new TransferCapabilitiesQuery(),ct)).List);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>tq.Handle(new TransferListQuery(),ct));
            Actor("hemo.bank.supervisor");var sc=await tq.Handle(new TransferCapabilitiesQuery(),ct);Assert.True(sc.Dispatch);Assert.False(sc.Start);
            var candidates=(await tq.Handle(new DispatchCandidatesQuery(),ct)).Data;Assert.Equal(4,candidates.Count);
            DispatchInput Input(DispatchCandidate u)=>new(u.UnitGuid,u.UnitVersion,u.UnitCode,u.WristbandCode,"Fixture courier","FIX","FIX-BOX",0m,"Fixture indicator",null,31,"Synthetic fixture only");
            var one=candidates.Single(x=>x.UnitCode=="DEMO-UNIT-01");
            await Assert.ThrowsAsync<ServiceExeption>(()=>tx.Handle(new DispatchTransferCommand(Input(one) with{UnitCode="WRONG"}),ct));
            db.ChangeTracker.Clear();
            var id=await tx.Handle(new DispatchTransferCommand(Input(one)),ct);
            db.ChangeTracker.Clear();
            await Assert.ThrowsAsync<ServiceExeption>(()=>tx.Handle(new DispatchTransferCommand(Input(one)),ct));
            var two=candidates.Single(x=>x.UnitCode=="DEMO-UNIT-02");
            db.ChangeTracker.Clear();var returnId=await tx.Handle(new DispatchTransferCommand(Input(two)),ct);
            var three=candidates.Single(x=>x.UnitCode=="DEMO-UNIT-03");
            db.ChangeTracker.Clear();var stopId=await tx.Handle(new DispatchTransferCommand(Input(three)),ct);
            var four=candidates.Single(x=>x.UnitCode=="DEMO-UNIT-04");
            Actor("hemo.nurse");await Assert.ThrowsAsync<ForbiddenAccessException>(()=>tx.Handle(new DispatchTransferCommand(Input(four)),ct));
            async Task<TransferDetail> Get(Guid i){db.ChangeTracker.Clear();return await tq.Handle(new TransferDetailQuery(i),ct);}
            async Task Act(Guid i,string action,Func<TransferActionInput,TransferActionInput>? edit=null){
                var d=await Get(i);var v=new TransferActionInput(i,d.Transfer.Version,d.Transfer.UnitCode,d.WristbandCode,31,"Synthetic fixture only",0m,"Acceptable","Fixture recipient","Fixture equipment");
                await tx.Handle(new TransferActionCommand(edit==null?v:edit(v),action),ct);
            }
            async Task Vital(Guid i,string phase){
                var d=await Get(i);await tx.Handle(new RecordTransferVitalCommand(new(i,d.Transfer.Version,phase,DateTimeOffset.UtcNow,120,80,75,37m,98,16,null,"Synthetic fixture observation")),ct);
            }
            Assert.True((await Get(id)).Demo);
            await Assert.ThrowsAsync<ServiceExeption>(()=>Act(id,"Start"));
            await Assert.ThrowsAsync<ServiceExeption>(()=>Act(id,"Receive",x=>x with{WristbandCode="WRONG"}));
            await Act(id,"Receive");
            await Act(id,"Verify");
            await Assert.ThrowsAsync<ServiceExeption>(()=>Act(id,"Witness"));
            Actor("hemo.nurse.witness");Assert.True((await tq.Handle(new TransferCapabilitiesQuery(),ct)).Witness);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>Act(id,"Start"));
            await Act(id,"Witness");
            Actor("hemo.nurse");
            await Assert.ThrowsAsync<ServiceExeption>(()=>Act(id,"Start"));
            await Vital(id,"T0");
            await Act(id,"Start");
            await Assert.ThrowsAsync<ServiceExeption>(()=>Act(id,"Complete"));
            await Vital(id,"T15");await Vital(id,"Mid");await Vital(id,"End");
            await Act(id,"Complete");await Vital(id,"Post1h");
            var done=await Get(id);Assert.Equal("Completed",done.Transfer.Status);Assert.Equal(5,done.Vitals.Count);
            Assert.NotEqual(done.VerifiedBy,done.WitnessedBy);
            Assert.Equal("Transfused",await db.BankBloodUnit.Where(x=>x.GUID==one.UnitGuid).Select(x=>x.Status).SingleAsync(ct));
            await Assert.ThrowsAsync<ServiceExeption>(()=>Act(id,"Return"));
            await Act(returnId,"ReceiptHold",x=>x with{UnitCode="OBSERVED-MISMATCH",Condition="Mismatch"});
            Assert.Equal("ReceiptHold",(await Get(returnId)).Transfer.Status);
            await Assert.ThrowsAsync<ServiceExeption>(()=>Act(returnId,"Verify"));
            await Act(returnId,"Return",x=>x with{Condition="Fixture return"});
            Actor("hemo.bloodbank");await Act(returnId,"AcceptReturn",x=>x with{Condition="Fixture returned"});
            Assert.Equal("ReturnedHold",await db.BankBloodUnit.Where(x=>x.GUID==two.UnitGuid).Select(x=>x.Status).SingleAsync(ct));
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>Act(returnId,"Dispose"));
            Actor("hemo.bank.supervisor");await Act(returnId,"Dispose");
            Assert.Equal("ReturnedDisposed",await db.BankBloodUnit.Where(x=>x.GUID==two.UnitGuid).Select(x=>x.Status).SingleAsync(ct));
            Actor("hemo.nurse");await Act(stopId,"Receive");await Act(stopId,"Verify");
            Actor("hemo.nurse.witness");await Act(stopId,"Witness");
            Actor("hemo.nurse");await Vital(stopId,"T0");await Act(stopId,"Start");
            Actor("hemo.doctor");await Act(stopId,"DoctorNote",x=>x with{SuspectedReaction=true});
            await Act(stopId,"Stop",x=>x with{SuspectedReaction=true});
            Assert.True((await Get(stopId)).Transfer.SuspectedReaction);
            Actor("hemo.nurse");await Assert.ThrowsAsync<ServiceExeption>(()=>Act(stopId,"Start"));
            await Act(stopId,"Return");
            Actor("hemo.bloodbank");await Act(stopId,"AcceptReturn");
            // Equipment concerns must not overwrite the state of an issued or consumed unit.
            var bank=new BankCommandHandler(new BankBloodUnitService(db,null!),roles,auth);
            await bank.Handle(new RecordBankTemperatureCommand(new("DEMO-ST05-FRIDGE",DateTimeOffset.UtcNow,0m,"Concern","Synthetic fixture concern")),ct);
            Assert.Equal("Transfused",await db.BankBloodUnit.Where(x=>x.GUID==one.UnitGuid).Select(x=>x.Status).SingleAsync(ct));
            Assert.Equal("ReturnedHold",await db.BankBloodUnit.Where(x=>x.GUID==three.UnitGuid).Select(x=>x.Status).SingleAsync(ct));
            // Change admission rowversion after the bank has prepared the unit.
            Actor("hemo.admission");var admissionGuid=await db.BloodRequest.Where(x=>x.GUID==four.RequestGuid).Select(x=>x.PatientAdmissionGUID).SingleAsync(ct);
            var a=await admissionQuery.Handle(new PatientAdmissionFindByIdQuery(admissionGuid),ct);
            await review.Handle(new ReviewAdmissionCommand(admissionGuid,"Identity","Verified",31,null,a.Version),ct);
            Actor("hemo.bank.supervisor");await Assert.ThrowsAsync<ServiceExeption>(()=>tx.Handle(new DispatchTransferCommand(Input(four)),ct));
            var stats=await tq.Handle(new TransferStatsQuery(),ct);Assert.Equal(3,stats.Total);Assert.Equal(1,stats.Completed);Assert.Equal(1,stats.Returned);Assert.Equal(1,stats.Disposed);Assert.Equal(1,stats.SuspectedReactions);
            var actor=users.Single(x=>x.User.Username=="hemo.bank.supervisor");var other=new OrgLocation{Name="Other fixture center"};db.OrgLocation.Add(other);await db.SaveChangesAsync(ct);
            var assignment=new UserRole{UserGUID=actor.UserGUID,RoleGUID=actor.RoleGUID,OrgLocationGUID=other.GUID,ExpireDate=DateTime.Now.AddDays(1)};db.UserRole.Add(assignment);await db.SaveChangesAsync(ct);db.ChangeTracker.Clear();
            http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",actor.UserGUID.ToString()),new Claim("UserRoles",assignment.Gkey.ToString())},"Fixture"));http.Request.Headers["UserRole"]=assignment.Gkey.ToString();
            Assert.Empty((await tq.Handle(new TransferListQuery(),ct)).Data);await Assert.ThrowsAsync<ServiceExeption>(()=>tq.Handle(new TransferDetailQuery(id),ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }
}
