using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.BloodRequest_core;
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
public class BloodRequestSqlWorkflowTests
{
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
    public async Task SeededRolesExecuteRequestLifecycleAndPreventUnauthorizedDecisions()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST03_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new HttpContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var secrets=HemoDemoSeed.Accounts.Concat(HemoDemoSeed.BloodRequestAccounts).ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeBloodRequests"]="true";
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
            Actor("hemo.admission");
            var admission=await admissionCreate.Handle(new CreatePatientAdmissionCommand{IdentificationType="Unknown",FirstName="Fixture",LastName="Patient",HisNumber="ST03-01",Ward="Fixture",BedNumber="1",Physician="Fixture"},ct);
            var a=await admissionQuery.Handle(new PatientAdmissionFindByIdQuery(admission),ct);
            await review.Handle(new ReviewAdmissionCommand(admission,"Identity","Verified",31,null,a.Version),ct);
            Actor("hemo.nurse");var nurseCaps=await query.Handle(new BloodRequestCapabilitiesQuery(),ct);
            Assert.True(nurseCaps.NurseCreate);Assert.False(nurseCaps.Decide);Assert.False(nurseCaps.BankQueue);
            var input=new RequestInput(admission,Guid.NewGuid(),"RBC","Routine",new(){new("RBC",1)},new(ClinicalIndication:"Fixture indication"));
            var guid=await commands.Handle(new CreateNurseRequestCommand(input),ct);
            Assert.Equal(guid,await commands.Handle(new CreateNurseRequestCommand(input),ct));
            var d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.Equal("Draft",d.Request.Status);Assert.True(d.Request.Owned);
            await commands.Handle(new UpdateBloodRequestCommand(guid,d.Version,input with{Items=new(){new("RBC",2),new("FFP",1)}}),ct);
            db.ChangeTracker.Clear();d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.Equal(2,d.Items.Count);Assert.Equal(3,d.Request.TotalUnits);
            await commands.Handle(new UpdateBloodRequestCommand(guid,d.Version,input),ct);
            db.ChangeTracker.Clear();d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.Single(d.Items);
            await commands.Handle(new UpdateBloodRequestCommand(guid,d.Version,input with{Items=new(){new("RBC",2),new("FFP",1)}}),ct);
            db.ChangeTracker.Clear();d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.Equal(2,d.Items.Count);
            await commands.Handle(new SubmitBloodRequestCommand(new(guid,d.Version,"Submit")),ct);
            db.ChangeTracker.Clear();d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.Equal("AwaitingDoctor",d.Request.Status);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>commands.Handle(new DecideBloodRequestCommand(new(guid,d.Version,"Approve",null,new("D","I","M",null,true))),ct));
            await Assert.ThrowsAsync<ServiceExeption>(()=>commands.Handle(new UpdateBloodRequestCommand(guid,d.Version,input),ct));
            Actor("hemo.bloodbank");var bankCaps=await query.Handle(new BloodRequestCapabilitiesQuery(),ct);
            Assert.True(bankCaps.BankQueue);Assert.False(bankCaps.List);Assert.False(bankCaps.Decide);
            Assert.Empty((await query.Handle(new BloodRequestBankQueueQuery(),ct)).Data);
            await Assert.ThrowsAsync<ServiceExeption>(()=>query.Handle(new BloodRequestBankDetailQuery(guid),ct));
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>query.Handle(new BloodRequestListQuery(),ct));
            Actor("hemo.doctor");var doctorCaps=await query.Handle(new BloodRequestCapabilitiesQuery(),ct);
            Assert.True(doctorCaps.Decide);Assert.False(doctorCaps.NurseCreate);
            d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.False(d.Request.Owned);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>commands.Handle(new CancelBloodRequestCommand(new(guid,d.Version,"Cancel","Not owner")),ct));
            await commands.Handle(new DecideBloodRequestCommand(new(guid,d.Version,"Return","Fixture correction")),ct);
            Actor("hemo.nurse");d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.Equal("Returned",d.Request.Status);
            var stale=d.Version;
            await commands.Handle(new UpdateBloodRequestCommand(guid,d.Version,input with{Clinical=new(ClinicalIndication:"Corrected")}),ct);
            db.ChangeTracker.Clear();d=await query.Handle(new BloodRequestDetailQuery(guid),ct);
            await Assert.ThrowsAsync<ServiceExeption>(()=>commands.Handle(new SubmitBloodRequestCommand(new(guid,stale,"Submit")),ct));
            await commands.Handle(new SubmitBloodRequestCommand(new(guid,d.Version,"Submit")),ct);
            Actor("hemo.admission");
            a=await admissionQuery.Handle(new PatientAdmissionFindByIdQuery(admission),ct);
            await review.Handle(new ReviewAdmissionCommand(admission,"Identity","Verified",31,null,a.Version),ct);
            Actor("hemo.doctor");d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.True(d.AdmissionChanged);
            await Assert.ThrowsAsync<ServiceExeption>(()=>commands.Handle(new DecideBloodRequestCommand(new(guid,d.Version,"Approve",null,new("D","I","M",null,true))),ct));
            await commands.Handle(new DecideBloodRequestCommand(new(guid,d.Version,"Return","Admission changed")),ct);
            Actor("hemo.nurse");d=await query.Handle(new BloodRequestDetailQuery(guid),ct);
            await commands.Handle(new SubmitBloodRequestCommand(new(guid,d.Version,"Submit")),ct);
            Actor("hemo.doctor");d=await query.Handle(new BloodRequestDetailQuery(guid),ct);
            await Assert.ThrowsAsync<ServiceExeption>(()=>commands.Handle(new DecideBloodRequestCommand(new(guid,d.Version,"Approve")),ct));
            await commands.Handle(new DecideBloodRequestCommand(new(guid,d.Version,"Approve",null,new("Fixture diagnosis","Fixture indication","FIX-ONLY","Fixture note",true))),ct);
            Actor("hemo.bloodbank");
            Assert.Equal(guid,Assert.Single((await query.Handle(new BloodRequestBankQueueQuery(),ct)).Data).Guid);
            d=await query.Handle(new BloodRequestBankDetailQuery(guid),ct);Assert.Equal("AwaitingBank",d.Request.Status);
            Assert.Equal("FIX-ONLY",d.History.Last().Physician!.MedicalCouncilNumber);Assert.Equal(3,d.History.Last().SubmissionAttempt);
            Assert.Equal(6,d.History.Count);
            Assert.Equal(6,await db.RequestApproval.CountAsync(x=>x.BloodRequestGUID==guid&&x.SnapshotJson!=null,ct));
            // A different center assignment, even with the same bank role, cannot see the request.
            var bankRole=users.Single(x=>x.User.Username=="hemo.bloodbank");
            var other=new OrgLocation{Name="Other fixture hospital"};db.OrgLocation.Add(other);await db.SaveChangesAsync(ct);
            var assignment=new UserRole{UserGUID=bankRole.UserGUID,RoleGUID=bankRole.RoleGUID,OrgLocationGUID=other.GUID,ExpireDate=DateTime.Now.AddDays(1)};
            db.UserRole.Add(assignment);await db.SaveChangesAsync(ct);
            db.ChangeTracker.Clear();
            http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",bankRole.UserGUID.ToString()),new Claim("UserRoles",assignment.Gkey.ToString())},"Fixture"));http.Request.Headers["UserRole"]=assignment.Gkey.ToString();
            Assert.Empty((await query.Handle(new BloodRequestBankQueueQuery(),ct)).Data);
            await Assert.ThrowsAsync<ServiceExeption>(()=>query.Handle(new BloodRequestBankDetailQuery(guid),ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }
}
