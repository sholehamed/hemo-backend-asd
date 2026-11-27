using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.BloodRequest_core;
using Application.Core.BloodBank_core;
using Application.Core.Transfusion_core;
using Application.Core.Hemovigilance_core;
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
public class HvSqlWorkflowTests
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
    public async Task ReactionFromSeedThroughIndependentCapaAndArchivedReport()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST06_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new FixtureContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var secrets=HemoDemoSeed.DemoAccounts(true,true,true,true).ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeHemovigilanceSamples"]="true";
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
            var service=new HemoReactionService(db,null!);var hv=new HvCommandHandler(service,roles,auth);var hq=new HvQueryHandler(service,roles,auth);
            Assert.Equal(4,await db.BloodTransfer.CountAsync(x=>x.HisNumber.StartsWith("DEMO-ST06-"),ct));Assert.Equal(3,await db.HemoReaction.CountAsync(ct));Assert.Equal(1,await db.HemoCapa.CountAsync(ct));
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HemoSeed:IncludeHemovigilanceSamples","true"}}).Build(),ct);
            Assert.Equal(4,await db.BloodTransfer.CountAsync(x=>x.HisNumber.StartsWith("DEMO-ST06-"),ct));Assert.Equal(3,await db.HemoReaction.CountAsync(ct));
            Actor("hemo.viewer");Assert.False((await hq.Handle(new HvCapabilitiesQuery(),ct)).List);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>hq.Handle(new HvListQuery(new()),ct));
            Actor("hemo.nurse");var cap=await hq.Handle(new HvCapabilitiesQuery(),ct);Assert.True(cap.Report);Assert.False(cap.DoctorReview);Assert.False(cap.Close);
            var c=Assert.Single((await hq.Handle(new HvCandidatesQuery(),ct)).Data);
            Assert.Equal("DEMO-ST06-01",c.HisNumber);
            var input=new ReactionInput(c.Guid,c.Version,Guid.NewGuid(),DateTimeOffset.UtcNow.AddMinutes(-1),new[]{"Unknown"},"Unknown","Synthetic observation","Synthetic record");
            await Assert.ThrowsAsync<ServiceExeption>(()=>hv.Handle(new ReportReactionCommand(input with{OnsetAt=DateTimeOffset.UtcNow.AddDays(-1)}),ct));
            db.ChangeTracker.Clear();var id=await hv.Handle(new ReportReactionCommand(input),ct);db.ChangeTracker.Clear();
            Assert.Equal(id,await hv.Handle(new ReportReactionCommand(input),ct));
            db.ChangeTracker.Clear();var tx=await db.BloodTransfer.AsNoTracking().SingleAsync(x=>x.GUID==c.Guid,ct);
            Assert.True(tx.SuspectedReaction);Assert.Equal("Stopped",tx.Status);
            await Assert.ThrowsAsync<ServiceExeption>(()=>hv.Handle(new ReportReactionCommand(input with{ClientReportId=Guid.NewGuid(),TransferVersion=tx.Version}),ct));
            async Task<HvDetail> Get(Guid guid){db.ChangeTracker.Clear();return await hq.Handle(new HvDetailQuery(guid),ct);}
            async Task Review(Guid guid,string action,Func<ReviewInput,ReviewInput>? edit=null){
                var d=await Get(guid);var v=new ReviewInput(guid,d.Case.Version,"Synthetic fixture only");await hv.Handle(new ReviewReactionCommand(edit==null?v:edit(v),action),ct);
            }
            var first=await Get(id);Assert.True(first.Case.Demo);Assert.Single(first.Vitals);
            await Review(id,"Observe");
            await Assert.ThrowsAsync<ServiceExeption>(()=>hv.Handle(new ReviewReactionCommand(new(id,first.Case.Version,"Stale"),"Observe"),ct));
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>Review(id,"DoctorReview"));
            Actor("hemo.bloodbank");await Assert.ThrowsAsync<ServiceExeption>(()=>Review(id,"BankReview"));
            Actor("hemo.doctor");await Review(id,"DoctorReview",x=>x with{DiagnosisCode="Undetermined",Severity="Unknown",Imputability="Unknown",Outcome="Unknown",Actions="Synthetic action",DoctorReference="DEMO-ONLY"});
            Actor("hemo.bloodbank");await Review(id,"BankReview",x=>x with{SampleReference="Synthetic sample",ReportReference="Synthetic report",Disposition="Synthetic disposition"});
            Actor("hemo.hv.officer");var officerCaps=await hq.Handle(new HvCapabilitiesQuery(),ct);Assert.True(officerCaps.Investigate);Assert.False(officerCaps.Close);
            // Return for re-review retains the old records in append-only history.
            await Review(id,"ReturnToDoctor");Assert.Null((await Get(id)).Case.DiagnosisCode);
            Actor("hemo.doctor");await Review(id,"DoctorReview",x=>x with{DiagnosisCode="Undetermined",Severity="Unknown",Imputability="Unknown",Outcome="Unknown",Actions="Synthetic action",DoctorReference="DEMO-ONLY"});
            Actor("hemo.bloodbank");await Review(id,"BankReview",x=>x with{SampleReference="Synthetic sample",ReportReference="Synthetic report",Disposition="Synthetic disposition"});
            Actor("hemo.hv.officer");await Review(id,"Investigate",x=>x with{RcaRequired=true,ExternalStatus="Required"});
            await Assert.ThrowsAsync<ServiceExeption>(()=>Review(id,"PublishRca",x=>x with{RcaLead="Fixture",RcaMethod="FiveWhys",MeetingAt=DateTimeOffset.UtcNow,Participants="Fixture",DirectCause="Fixture",RootCause="Fixture"}));
            var owners=await hq.Handle(new HvOwnersQuery(),ct);var nurse=users.Single(x=>x.User.Username=="hemo.nurse");Assert.Contains(owners,x=>x.UserGuid==nurse.UserGUID);
            var d=await Get(id);var action=await hv.Handle(new AssignCapaCommand(new(id,d.Case.Version,"Fixture action","Corrective",nurse.UserGUID,DateTimeOffset.UtcNow.AddDays(1),"Fixture indicator")),ct);
            await Review(id,"PublishRca",x=>x with{RcaLead="Fixture",RcaMethod="FiveWhys",MeetingAt=DateTimeOffset.UtcNow,Participants="Fixture",DirectCause="Fixture",RootCause="Fixture"});
            Actor("hemo.hv.reviewer");await Assert.ThrowsAsync<ServiceExeption>(()=>Review(id,"Close"));
            async Task Capa(string a,int p){var current=await Get(id);var row=current.Actions.Single(x=>x.Guid==action);await hv.Handle(new CapaActionCommand(new(row.Guid,row.Version,current.Case.Version,p,"Synthetic evidence"),a),ct);}
            Actor("hemo.doctor");await Assert.ThrowsAsync<ServiceExeption>(()=>Capa("CapaProgress",100));
            Actor("hemo.nurse");await Capa("CapaProgress",40);await Capa("CapaProgress",100);await Assert.ThrowsAsync<ForbiddenAccessException>(()=>Capa("CapaVerify",100));
            Actor("hemo.hv.reviewer");await Capa("CapaReject",100);
            Actor("hemo.nurse");await Capa("CapaProgress",100);
            Actor("hemo.hv.reviewer");await Capa("CapaVerify",100);await Assert.ThrowsAsync<ServiceExeption>(()=>Review(id,"Close"));
            Actor("hemo.hv.officer");await Assert.ThrowsAsync<ServiceExeption>(()=>Review(id,"External",x=>x with{ExternalStatus="SentManually"}));
            await Review(id,"External",x=>x with{ExternalStatus="SentManually",ExternalReference="DEMO-EXTERNAL"});
            var report=await hv.Handle(new ArchiveHvReportCommand(new("Synthetic report",DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow)),ct);
            var archived=await hq.Handle(new HvReportDetailQuery(report),ct);Assert.Equal(4,archived.Summary.Total);Assert.Equal(0,archived.Summary.Closed);Assert.Equal(4,archived.Summary.StartedTransfers);
            await hv.Handle(new RecordReportExternalCommand(new(report,archived.Report.Version,"SentManually","DEMO-REPORT")),ct);
            Actor("hemo.hv.reviewer");await Review(id,"Close");var done=await Get(id);Assert.Equal("Closed",done.Case.Status);Assert.Equal("Verified",Assert.Single(done.Actions).Status);Assert.Contains(done.Events,e=>e.Action=="ReturnToDoctor");
            Assert.Equal(0,(await hq.Handle(new HvReportDetailQuery(report),ct)).Summary.Closed);Assert.Equal(1,(await hq.Handle(new HvStatsQuery(new()),ct)).Closed);
            Actor("hemo.nurse");await Assert.ThrowsAsync<ServiceExeption>(()=>Review(id,"Observe"));
            var later=Assert.Single((await hq.Handle(new HvCandidatesQuery(Search:"DEMO-ST06-01"),ct)).Data);Assert.Equal(c.Guid,later.Guid);
            var gate=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            async Task<Guid?> Compete()
            {
                var context=new DefaultHttpContext();
                context.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",nurse.UserGUID.ToString()),new Claim("UserRoles",nurse.Gkey.ToString())},"Fixture"));
                context.Request.Headers["UserRole"]=nurse.Gkey.ToString();
                var separateAuth=new AuthService(new FixtureContextAccessor{HttpContext=context});
                var separateOptions=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString).AddInterceptors(new FixtureAudit(separateAuth),new DispatchDomainEventsInterceptor(null!)).Options;
                await using var separateDb=new ApplicationDbContext(separateOptions,separateAuth);
                var handler=new HvCommandHandler(new HemoReactionService(separateDb,null!),new UserRoleService(separateDb,null!,separateAuth),separateAuth);
                await gate.Task;
                try{return await handler.Handle(new ReportReactionCommand(input with{ClientReportId=Guid.NewGuid(),TransferVersion=later.Version,OnsetAt=DateTimeOffset.UtcNow}),ct);}
                catch(ServiceExeption){return null;}
            }
            var firstTask=Compete();var secondTask=Compete();gate.SetResult(true);
            var outcomes=await Task.WhenAll(firstTask,secondTask);var next=Assert.Single(outcomes.Where(x=>x.HasValue))!.Value;
            db.ChangeTracker.Clear();Assert.Equal(1,await db.HemoReaction.CountAsync(x=>x.BloodTransferGUID==c.Guid&&x.Status!="Closed",ct));
            Assert.Contains((await hq.Handle(new HvListQuery(new(Status:"CapaMonitoring",MineCapa:true)),ct)).Data,x=>x.CaseNumber=="DEMO-HV-04");
            Actor("hemo.doctor");await Review(next,"DoctorReview",x=>x with{DiagnosisCode="Undetermined",Severity="Unknown",Imputability="Unknown",Outcome="Unknown",Actions="Synthetic action",DoctorReference="DEMO-ONLY"});
            Actor("hemo.bloodbank");await Review(next,"BankReview",x=>x with{SampleReference="Synthetic sample",ReportReference="Synthetic report",Disposition="Synthetic disposition"});
            Actor("hemo.hv.officer");Assert.Contains((await hq.Handle(new HvListQuery(new(Status:"RcaQueue")),ct)).Data,x=>x.Guid==next);
            await Review(next,"Investigate",x=>x with{RcaRequired=false,ExternalStatus="NotRequired"});
            Actor("hemo.hv.reviewer");await Review(next,"Close");Assert.Equal("Closed",(await Get(next)).Case.Status);
            Assert.Equal(2,(await hq.Handle(new HvStatsQuery(new()),ct)).Closed);
            var actor=users.Single(x=>x.User.Username=="hemo.hv.reviewer");var other=new OrgLocation{Name="Other fixture center"};db.OrgLocation.Add(other);await db.SaveChangesAsync(ct);
            var assignment=new UserRole{UserGUID=actor.UserGUID,RoleGUID=actor.RoleGUID,OrgLocationGUID=other.GUID,ExpireDate=DateTime.Now.AddDays(1)};db.UserRole.Add(assignment);await db.SaveChangesAsync(ct);db.ChangeTracker.Clear();
            http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",actor.UserGUID.ToString()),new Claim("UserRoles",assignment.Gkey.ToString())},"Fixture"));http.Request.Headers["UserRole"]=assignment.Gkey.ToString();
            Assert.Empty((await hq.Handle(new HvListQuery(new()),ct)).Data);await Assert.ThrowsAsync<ServiceExeption>(()=>hq.Handle(new HvDetailQuery(id),ct));await Assert.ThrowsAsync<ServiceExeption>(()=>hq.Handle(new HvReportDetailQuery(report),ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }
}
