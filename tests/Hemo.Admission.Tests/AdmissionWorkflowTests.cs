using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.PatientAdmission_core.Commands.CreatePatientAdmission;
using Application.Core.PatientAdmission_core.Commands.ReviewAdmission;
using Application.Core.PatientAdmission_core.Commands.UpdateAdmission;
using Application.Core.PatientAdmission_core.Commands.UpdatePatientIdentity;
using Application.Core.PatientAdmission_core.Queries;
using Application.Repository.Services;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Hemo.Admission.Tests;
public class AdmissionWorkflowTests
{
    // Test-only audit stamping: the production interceptor and its Mongo logging are unchanged.
    private sealed class FixtureAudit(AuthService auth):SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e,InterceptionResult<int> result,CancellationToken ct=default)
        {
            foreach(var entry in e.Context!.ChangeTracker.Entries<AuditedEntity>())
            {
                if(entry.State==EntityState.Added){entry.Entity.Created=DateTime.UtcNow;entry.Entity.CreatedBy=auth.GetUserGUID();entry.Entity.CreatedByIP="test";}
                if(entry.State is EntityState.Added or EntityState.Modified){entry.Entity.LastModified=DateTime.UtcNow;entry.Entity.LastModifiedBy=auth.GetUserGUID();entry.Entity.LastModifiedByIP="test";}
            }
            return ValueTask.FromResult(result);
        }
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task AdmissionLifecyclePersistsAndEnforcesFacilityAndApiScope()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST02_flow_"+Guid.NewGuid().ToString("N")};
        var actor=new User{Username="test-admission",Password="fixture-not-a-login",FirstName="Test",LastName="Operator"};
        var http=new DefaultHttpContext();
        http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",actor.GUID.ToString()),new Claim("UserRoles","910001"),new Claim("UserRoles","910002"),new Claim("UserRoles","910003")},"Fixture"));
        http.Request.Headers["UserRole"]="910001";
        var auth=new AuthService(new HttpContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString).AddInterceptors(new FixtureAudit(auth)).Options;
        await using var db=new ApplicationDbContext(options,auth);
        var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var orgA=new OrgLocation{Name="Fixture A",Gkey=910001,Path="/910001/",Level=1};
            var orgB=new OrgLocation{Name="Fixture B",Gkey=910002,Path="/910002/",Level=1};
            var admin=await db.Role.SingleOrDefaultAsync(x=>x.Gkey==1,ct)??new Role{Name="Fixture admin",Gkey=1,Path="/1/",Level=1};
            if(db.Entry(admin).State==EntityState.Detached)db.Role.Add(admin);
            var reader=new Role{Name="Fixture reader",Gkey=910003,Path="/1/910003/",Level=2};
            db.AddRange(actor,orgA,orgB,reader);
            db.UserRole.AddRange(new UserRole{UserGUID=actor.GUID,RoleGUID=admin.GUID,OrgLocationGUID=orgA.GUID,Gkey=910001,Sign="fixture",ExpireDate=DateTime.Now.AddDays(1)},
                new UserRole{UserGUID=actor.GUID,RoleGUID=admin.GUID,OrgLocationGUID=orgB.GUID,Gkey=910002,Sign="fixture",ExpireDate=DateTime.Now.AddDays(1)},
                new UserRole{UserGUID=actor.GUID,RoleGUID=reader.GUID,OrgLocationGUID=orgA.GUID,Gkey=910003,Sign="fixture",ExpireDate=DateTime.Now.AddDays(1)});
            var subsystem=new SubSystem{Name="Fixture",Host="localhost",Port=1002,Active=true,Created=DateTime.UtcNow};
            var api=new Api{SubSystemGUID=subsystem.GUID,APITypeName="POST",DownstreamPathTemplate="/Identity/PatientAdmission/Stats",UpstreamPathTemplate="/Identity/PatientAdmission/Stats",Active=true,Title="Fixture stats"};
            var menu=new Menu{label="Fixture",routerLink="/fixture-st02",Gkey=910001,Path="/910001/",Level=1,Created=DateTime.UtcNow};
            db.AddRange(subsystem,api,menu,new RoleAccess{RoleGUID=reader.GUID,MenuGUID=menu.GUID,ApiGUID=api.GUID,Gkey=910001,Path="/910001/",Level=1});
            await db.SaveChangesAsync(ct);
            var patients=new PatientService(db,null!);
            var admissions=new PatientAdmissionService(db,null!);
            var reviews=new PatientAdmissionReviewService(db,null!);
            var roles=new UserRoleService(db,null!,auth);
            var create=new CreatePatientAdmissionCommandHandler(patients,admissions,roles,auth);
            var query=new PatientAdmissionQueryHandler(admissions,patients,reviews,roles,auth);
            var review=new ReviewAdmissionCommandHandler(admissions,reviews,roles,auth);
            var update=new UpdateAdmissionCommandHandler(admissions,roles,auth);
            var identity=new UpdatePatientIdentityCommandHandler(admissions,patients,reviews,roles,auth);
            var command=new CreatePatientAdmissionCommand{IdentificationType="Unknown",FirstName="Test",LastName="Unknown",HisNumber="FIX-01",Ward="Fixture",BedNumber="1",Physician="Fixture"};
            var guid=await create.Handle(command,ct);
            var view=await query.Handle(new PatientAdmissionFindByIdQuery(guid),ct);
            Assert.StartsWith("UNK-",view.IdentificationNumber);
            Assert.StartsWith("HEMO-",view.WristbandCode);
            await Assert.ThrowsAsync<ServiceExeption>(()=>create.Handle(command,ct));
            await Assert.ThrowsAsync<ServiceExeption>(()=>review.Handle(new ReviewAdmissionCommand(guid,"Clearance","Ready",31,null,view.Version),ct));
            await identity.Handle(new UpdatePatientIdentityCommand(guid,"Passport","FIX-PASSPORT","Test","Resolved",null,new DateTime(1980,1,1),"Unknown",null,"Fixture resolution",view.Version),ct);
            view=await query.Handle(new PatientAdmissionFindByIdQuery(guid),ct);
            Assert.Equal("FIX-PASSPORT",view.IdentificationNumber);
            Assert.Equal("Pending",view.IdentityStatus);
            var patientGuid=view.PatientGuid;
            var found=await query.Handle(new PatientFindByIdentityQuery("Passport","fix-passport"),ct);
            Assert.Equal(patientGuid,found!.Guid);
            await update.Handle(new UpdateAdmissionCommand(guid,"Updated ward","2","Fixture",null,null,null,view.Version),ct);
            var oldVersion=view.Version;
            view=await query.Handle(new PatientAdmissionFindByIdQuery(guid),ct);
            Assert.Equal("Updated ward",view.Ward);
            await Assert.ThrowsAsync<ServiceExeption>(()=>update.Handle(new UpdateAdmissionCommand(guid,"Stale","1","Fixture",null,null,null,oldVersion),ct));
            await review.Handle(new ReviewAdmissionCommand(guid,"Identity","Verified",31,null,view.Version),ct);
            view=await query.Handle(new PatientAdmissionFindByIdQuery(guid),ct);
            await review.Handle(new ReviewAdmissionCommand(guid,"History","Reviewed",0,"Fixture review",view.Version),ct);
            view=await query.Handle(new PatientAdmissionFindByIdQuery(guid),ct);
            await review.Handle(new ReviewAdmissionCommand(guid,"Clearance","Ready",31,null,view.Version),ct);
            view=await query.Handle(new PatientAdmissionFindByIdQuery(guid),ct);
            Assert.Equal("ReadyForDischarge",view.Status);
            Assert.Equal(4,(await query.Handle(new PatientAdmissionReviewsQuery(guid),ct)).Count);
            var second=await create.Handle(command with{IdentificationType="Passport",IdentificationNumber="FIX-PASSPORT",FirstName="Test",LastName="Resolved",BirthDate=new DateTime(1980,1,1),HisNumber="FIX-02"},ct);
            Assert.Equal(patientGuid,(await query.Handle(new PatientAdmissionFindByIdQuery(second),ct)).PatientGuid);
            Assert.Equal(2,(await query.Handle(new PatientAdmissionHistoryQuery(guid),ct)).Count);
            Assert.Equal(2,(await query.Handle(new PatientAdmissionStatsQuery(),ct)).Total);
            http.Request.Headers["UserRole"]="910002";
            Assert.Equal(0,(await query.Handle(new PatientAdmissionStatsQuery(),ct)).Total);
            await Assert.ThrowsAsync<ServiceExeption>(()=>query.Handle(new PatientAdmissionFindByIdQuery(guid),ct));
            http.Request.Headers["UserRole"]="910003";
            Assert.Equal(2,(await query.Handle(new PatientAdmissionStatsQuery(),ct)).Total);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>create.Handle(command with{HisNumber="FIX-03"},ct));
        }
        finally {await db.Database.EnsureDeletedAsync(ct);}
    }
}
