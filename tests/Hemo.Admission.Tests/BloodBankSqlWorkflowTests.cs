using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Utility.Security;
using Application.Core.BloodRequest_core;
using Application.Core.BloodBank_core;
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
public class BloodBankSqlWorkflowTests
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
    public async Task InventoryLaboratoryReservationThawTemperatureAndReturnAreScoped()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_ST04_"+Guid.NewGuid().ToString("N")};
        var http=new DefaultHttpContext();var auth=new AuthService(new HttpContextAccessor{HttpContext=http});
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new FixtureAudit(auth),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var secrets=HemoDemoSeed.DemoAccounts(true,true).ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeBloodBank"]="true";
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
            Actor("hemo.nurse");
            var input=new RequestInput(admission,Guid.NewGuid(),"General","Routine",new(){new("RBC",1),new("FFP",1)},new(ClinicalIndication:"Fixture only"));
            var guid=await commands.Handle(new CreateNurseRequestCommand(input),ct);
            var d=await query.Handle(new BloodRequestDetailQuery(guid),ct);await commands.Handle(new SubmitBloodRequestCommand(new(guid,d.Version,"Submit")),ct);
            Actor("hemo.doctor");d=await query.Handle(new BloodRequestDetailQuery(guid),ct);
            await commands.Handle(new DecideBloodRequestCommand(new(guid,d.Version,"Approve",null,new("Fixture","Fixture","FIX-ONLY",null,true))),ct);
            var bank=new BankCommandHandler(new BankBloodUnitService(db,null!),roles,auth);
            var bq=new BankQueryHandler(new BankBloodUnitService(db,null!),roles,auth);
            Actor("hemo.nurse");Assert.False((await bq.Handle(new BankCapabilitiesQuery(),ct)).Receive);
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>bq.Handle(new BankInventoryQuery(),ct));
            Actor("hemo.bloodbank");var caps=await bq.Handle(new BankCapabilitiesQuery(),ct);Assert.True(caps.Receive);Assert.False(caps.Inspect);Assert.False(caps.Discard);
            d=await query.Handle(new BloodRequestBankDetailQuery(guid),ct);
            var requestVersion=d.Version;var now=DateTimeOffset.UtcNow;
            var receive=new ReceiveUnit("FIX-RBC-1","RBC","O+",null,"Fixture supplier","FIX-FRIDGE",now.AddHours(-1),now.AddDays(2));
            var unit=await bank.Handle(new ReceiveBankUnitCommand(receive),ct);
            var extra=await bank.Handle(new ReceiveBankUnitCommand(receive with{UnitCode="FIX-RBC-2"}),ct);
            var frozen=await bank.Handle(new ReceiveBankUnitCommand(receive with{UnitCode="FIX-FFP-1",ProductCode="FFP",EquipmentCode="FIX-FREEZER"}),ct);
            await Assert.ThrowsAsync<ServiceExeption>(()=>bank.Handle(new ReceiveBankUnitCommand(receive),ct));
            async Task<UnitDetail> Get(Guid id){db.ChangeTracker.Clear();return await bq.Handle(new BankUnitQuery(id),ct);}
            async Task Action(Guid id,string action,string endpoint,Guid? req=null,byte[]? rv=null,string? method=null,decimal? temp=null,DateTimeOffset? expiry=null){
                var u=await Get(id);await bank.Handle(new BankUnitActionCommand(new(id,u.Unit.Version,action,req,rv,"Fixture action",method,temp,expiry),endpoint),ct);
            }
            await Assert.ThrowsAsync<ForbiddenAccessException>(()=>Action(unit,"Inspect","Inspect"));
            await bank.Handle(new RecordBankTemperatureCommand(new("FIX-FRIDGE",now,4m,"Acceptable","Fixture assessment")),ct);
            await bank.Handle(new RecordBankTemperatureCommand(new("FIX-FREEZER",now,-25m,"Acceptable","Fixture assessment")),ct);
            Actor("hemo.bank.supervisor");Assert.True((await bq.Handle(new BankCapabilitiesQuery(),ct)).Inspect);
            foreach(var id in new[]{unit,extra,frozen})await Action(id,"Inspect","Inspect");
            Actor("hemo.bloodbank");
            await Assert.ThrowsAsync<ServiceExeption>(()=>Action(unit,"Reserve","Reserve",guid,requestVersion));
            async Task Lab(Guid id,string conclusion="Accepted",string ahg="NonReactive"){
                var u=await Get(id);await bank.Handle(new RecordBankLabCommand(new(id,u.Unit.Version,guid,requestVersion,"FIX-SAMPLE","Confirmed","O+","Fixture method","NonReactive","NonReactive",ahg,"NonReactive","NonReactive","FIX-REPORT",conclusion,"Fixture result")),ct);
            }
            await Assert.ThrowsAsync<ServiceExeption>(()=>Lab(unit,"Accepted","Reactive"));
            await Lab(unit);await Lab(extra);await Lab(frozen);
            var stale=(await Get(unit)).Unit.Version;
            await Action(unit,"Reserve","Reserve",guid,requestVersion);
            await Assert.ThrowsAsync<ServiceExeption>(()=>bank.Handle(new BankUnitActionCommand(new(unit,stale,"Hold",Reason:"Stale"),"Hold"),ct));
            await Assert.ThrowsAsync<ServiceExeption>(()=>Action(extra,"Reserve","Reserve",guid,requestVersion));
            await Action(unit,"Ready","Ready");
            Assert.Equal("Ready",(await Get(unit)).Unit.Status);
            await Action(frozen,"Reserve","Reserve",guid,requestVersion);
            await Assert.ThrowsAsync<ServiceExeption>(()=>Action(frozen,"Ready","Ready"));
            await Action(frozen,"StartThaw","Thaw",method:"Fixture procedure");
            await Assert.ThrowsAsync<ServiceExeption>(()=>Action(frozen,"FinishThaw","Thaw",method:"Fixture",temp:35m,expiry:now.AddDays(3)));
            await Action(frozen,"FinishThaw","Thaw",method:"Fixture procedure",temp:35m,expiry:now.AddHours(2));
            await Action(frozen,"Ready","Ready");
            Assert.Equal("Ready",(await Get(frozen)).Unit.Status);
            var inv=await bq.Handle(new BankInventoryQuery(RequestGuid:guid),ct);Assert.Equal(2,inv.TotalCount);
            await bank.Handle(new RecordBankTemperatureCommand(new("FIX-FRIDGE",DateTimeOffset.UtcNow,9m,"Concern","Fixture concern")),ct);
            Assert.Equal("Quarantined",(await Get(unit)).Unit.Status);Assert.Equal("Quarantined",(await Get(extra)).Unit.Status);
            Actor("hemo.bank.supervisor");
            await Assert.ThrowsAsync<ServiceExeption>(()=>bank.Handle(new BankReturnRequestCommand(new(guid,requestVersion,"Fixture correction")),ct));
            await Action(unit,"Unreserve","Unreserve");await Action(frozen,"Unreserve","Unreserve");
            await Assert.ThrowsAsync<ServiceExeption>(()=>Action(frozen,"Inspect","Inspect"));
            await Action(frozen,"Discard","Discard");
            Assert.Equal("Discarded",(await Get(frozen)).Unit.Status);
            await bank.Handle(new BankReturnRequestCommand(new(guid,requestVersion,"Fixture correction")),ct);
            Actor("hemo.nurse");d=await query.Handle(new BloodRequestDetailQuery(guid),ct);Assert.Equal("Returned",d.Request.Status);Assert.Equal("BankReturn",d.History.Last().Action);
            Actor("hemo.bloodbank");Assert.Empty((await query.Handle(new BloodRequestBankQueueQuery(),ct)).Data);
            var bankRole=users.Single(x=>x.User.Username=="hemo.bloodbank");
            var other=new OrgLocation{Name="Other fixture bank"};db.OrgLocation.Add(other);await db.SaveChangesAsync(ct);
            var assignment=new UserRole{UserGUID=bankRole.UserGUID,RoleGUID=bankRole.RoleGUID,OrgLocationGUID=other.GUID,ExpireDate=DateTime.Now.AddDays(1)};
            db.UserRole.Add(assignment);await db.SaveChangesAsync(ct);db.ChangeTracker.Clear();
            http.User=new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim("Id",bankRole.UserGUID.ToString()),new Claim("UserRoles",assignment.Gkey.ToString())},"Fixture"));http.Request.Headers["UserRole"]=assignment.Gkey.ToString();
            Assert.Empty((await bq.Handle(new BankInventoryQuery(),ct)).Data);
            await Assert.ThrowsAsync<ServiceExeption>(()=>bq.Handle(new BankUnitQuery(unit),ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }
}
