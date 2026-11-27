using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Common.Utility.Security;
using Application.Core.BloodRequest_core;
using Application.Core.BloodBank_core;
using Application.Core.Hemovigilance_core;
using Application.Core.Quality_core;
using Application.Core.Transfusion_core;
using Application.Core.Transfusion_core.Queries;
using Application.Core.PatientAdmission_core.Commands.CreatePatientAdmission;
using Application.Core.PatientAdmission_core.Commands.ReviewAdmission;
using Application.Repository.Services;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using web.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace Hemo.Admission.Tests;

// Never runs company Program or its hard-coded database/external integrations.
// Real endpoint classes, application DI, JWT bearer validation, token store and SQL.
public class IsolatedHttpAcceptanceTests(ITestOutputHelper output)
{
    private sealed class Audit(AuthService auth):SaveChangesInterceptor
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
    public async Task ActualHttpEndpointsValidateTokensPermissionsAndAdmissionRequestWorkflow()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection);
        // Fail before connection unless the SQL target is explicitly loopback.
        Assert.Contains(cs.DataSource.ToLowerInvariant(),new[]{"localhost","localhost,1433","127.0.0.1","127.0.0.1,1433","tcp:localhost,1433","tcp:127.0.0.1,1433"});
        cs.InitialCatalog="Hemo_HTTP_"+Guid.NewGuid().ToString("N");
        var key=new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(64));
        const string issuer="hemo-isolated-http-test",audience="hemo-test";
        var builder=WebApplication.CreateBuilder(new WebApplicationOptions{EnvironmentName="Testing",Args=Array.Empty<string>()});
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(o=>o.Listen(IPAddress.Loopback,0));
        builder.AddApplicationServices();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<Audit>();
        builder.Services.AddScoped<DispatchDomainEventsInterceptor>();
        builder.Services.AddDbContext<ApplicationDbContext>((sp,o)=>o.UseSqlServer(cs.ConnectionString)
            .AddInterceptors(sp.GetRequiredService<Audit>(),sp.GetRequiredService<DispatchDomainEventsInterceptor>()));
        builder.Services.AddScoped<IApplicationDbContext>(sp=>sp.GetRequiredService<ApplicationDbContext>());
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>
        {
            o.MapInboundClaims=false;
            o.TokenValidationParameters=new TokenValidationParameters{
                ValidIssuer=issuer,ValidAudience=audience,IssuerSigningKey=key,
                ValidateIssuer=true,ValidateAudience=true,ValidateIssuerSigningKey=true,ValidateLifetime=true,ClockSkew=TimeSpan.Zero};
        });
        builder.Services.AddAuthorization();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        await using var app=builder.Build();
        app.UseExceptionHandler();
        // Surface unexpected routing/pipeline exceptions in the test output, without logging headers or tokens.
        app.Use(async (context,next)=>
        {
            try { await next(context); }
            catch(Exception ex) { output.WriteLine(ex.ToString()); throw; }
        });
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        new Web.Endpoints.PatientAdmission().Map(app);
        new Web.Endpoints.BloodRequest().Map(app);
        new Web.Endpoints.BloodBank().Map(app);
        new Web.Endpoints.Transfusion().Map(app);
        new Web.Endpoints.Hemovigilance().Map(app);
        new Web.Endpoints.HemoQuality().Map(app);
        new Web.Endpoints.HemoSession().Map(app);
        new Web.Endpoints.RoleAccess().Map(app);
        // Materialize real route metadata before any network request. The framework requires globally unique names.
        var endpoints=((IEndpointRouteBuilder)app).DataSources.SelectMany(s=>s.Endpoints).OfType<RouteEndpoint>().ToArray();
        var names=endpoints.Select(e=>e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName).Where(n=>n!=null).ToArray();
        Assert.Equal(names.Length,names.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(137,names.Count(n=>n!.StartsWith("HEMO.",StringComparison.Ordinal)));
        Assert.Equal(endpoints.Length,endpoints.SelectMany(e=>e.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.Select(m=>m+" "+e.RoutePattern.RawText)).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        var actors=new Dictionary<string,(Guid User,long Role,Guid TokenKey)>();
        var started=false;
        try
        {
            using(var scope=app.Services.CreateScope())
            {
                var db=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();
                var accounts=HemoDemoSeed.DemoAccounts(true,true,true,true,true).Append(HemoDemoSeed.DashboardManager);
                var settings=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
                settings["HemoSeed:IncludeR02Pages"]="true";
                settings["HemoSeed:IncludeDashboardSamples"]="true";
                await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(settings).Build(),CancellationToken.None);
                var users=await db.UserRole.AsNoTracking().Include(x=>x.User).ToListAsync();
                Assert.Equal(15,users.Count);
                var service=scope.ServiceProvider.GetRequiredService<UserService>();
                foreach(var u in users)
                {
                    // Actual company in-memory token registry; no password/Captcha login simulated.
                    var token=service.SetTokenKey(new SystemSetting{TokenExpireSeconds=900,RefreshTokenExpireSeconds=300},u.UserGUID,true);
                    actors.Add(u.User.Username,(u.UserGUID,u.Gkey,token.TokenKey));
                }
            }
            await app.StartAsync();started=true;
            using var client=new HttpClient{BaseAddress=new Uri(app.Urls.Single()),Timeout=TimeSpan.FromSeconds(60)};
            string Token(string actor,bool expired=false,bool unknownKey=false,bool expiredClaim=false)
            {
                var a=actors[actor];
                var claims=new[]{new Claim("Id",a.User.ToString()),new Claim("UserRoles",a.Role.ToString(CultureInfo.InvariantCulture)),
                    new Claim("TokenKey",(unknownKey?Guid.NewGuid():a.TokenKey).ToString()),
                    new Claim("ExpireDate",DateTime.Now.AddMinutes(expiredClaim?-2:10).ToString(CultureInfo.CurrentCulture))};
                var jwt=new JwtSecurityToken(issuer,audience,claims,DateTime.UtcNow.AddMinutes(-5),
                    DateTime.UtcNow.AddMinutes(expired?-1:10),new SigningCredentials(key,SecurityAlgorithms.HmacSha512));
                return new JwtSecurityTokenHandler().WriteToken(jwt);
            }
            void Actor(string name,bool expired=false,bool unknownKey=false,bool expiredClaim=false)
            {
                client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",Token(name,expired,unknownKey,expiredClaim));
                client.DefaultRequestHeaders.Remove("UserRole");
                client.DefaultRequestHeaders.Add("UserRole",actors[name].Role.ToString(CultureInfo.InvariantCulture));
            }
            async Task<JsonElement> Json(HttpResponseMessage response,HttpStatusCode expected=HttpStatusCode.OK)
            {
                using(response)
                {
                    var body=await response.Content.ReadAsStringAsync();
                    Assert.True(response.StatusCode==expected,$"Expected {expected}, received {response.StatusCode} for {response.RequestMessage?.RequestUri?.AbsolutePath}: {body}");
                    return JsonDocument.Parse(body).RootElement.Clone();
                }
            }
            async Task Expect(HttpResponseMessage response,HttpStatusCode expected)
            {
                using(response)Assert.True(response.StatusCode==expected,$"Expected {expected}, received {response.StatusCode} for {response.RequestMessage?.RequestUri?.AbsolutePath}: {await response.Content.ReadAsStringAsync()}");
            }
            const string admission="/Identity/PatientAdmission",requests="/Identity/BloodRequest",manager="hemo.dashboard.manager";
            var modules=new[]{"PatientAdmission","BloodRequest","BloodBank","Transfusion","Hemovigilance","HemoQuality"};
            foreach(var module in modules)await Expect(await client.GetAsync("/Identity/"+module+"/Capabilities"),HttpStatusCode.Unauthorized);
            Actor(manager,expired:true);await Expect(await client.GetAsync(admission+"/Capabilities"),HttpStatusCode.Unauthorized);
            Actor(manager,unknownKey:true);await Expect(await client.GetAsync(admission+"/Capabilities"),HttpStatusCode.Unauthorized);
            Actor(manager,expiredClaim:true);await Expect(await client.GetAsync(admission+"/Capabilities"),HttpStatusCode.Unauthorized);
            Actor(manager);
            foreach(var module in modules)
            {
                var capabilities=await Json(await client.GetAsync("/Identity/"+module+"/Capabilities"));
                var flags=module=="HemoQuality"?capabilities.GetProperty("permissions"):capabilities;
                Assert.True(flags.GetProperty("stats").GetBoolean());
                await Json(await client.PostAsJsonAsync("/Identity/"+module+"/Stats",new{}));
            }
            await Json(await client.PostAsJsonAsync("/Identity/HemoQuality/EquipmentStats",new{}));
            Assert.True((await Json(await client.PostAsJsonAsync("/Identity/RoleAccess/CheckPagePermission",new{Menu="/hemo/dashboard"}))).GetBoolean());
            Assert.False((await Json(await client.PostAsJsonAsync("/Identity/RoleAccess/CheckPagePermission",new{Menu="/admission/new"}))).GetBoolean());
            foreach(var path in new[]{admission+"/BindPaging",requests+"/BindPaging","/Identity/BloodBank/Inventory","/Identity/Transfusion/List","/Identity/Hemovigilance/List","/Identity/HemoQuality/EquipmentList","/Identity/HemoQuality/QcList"})
                await Expect(await client.PostAsJsonAsync(path,new{}),HttpStatusCode.Forbidden);
            client.DefaultRequestHeaders.Remove("UserRole");
            client.DefaultRequestHeaders.Add("UserRole",actors["hemo.admission"].Role.ToString(CultureInfo.InvariantCulture));
            await Expect(await client.PostAsJsonAsync(admission+"/BindPaging",new{}),HttpStatusCode.BadRequest);

            Actor("hemo.admission");
            var input=new CreatePatientAdmissionCommand{IdentificationType="Unknown",FirstName="Synthetic",LastName="HTTP",HisNumber="FIX-HTTP-01",Ward="Fixture",BedNumber="1",Physician="Fixture"};
            var id=(await Json(await client.PostAsJsonAsync(admission,input),HttpStatusCode.Created)).GetGuid();
            Assert.NotEqual(Guid.Empty,id);
            var detail=await Json(await client.GetAsync(admission+"/"+id));
            Assert.Equal("FIX-HTTP-01",detail.GetProperty("hisNumber").GetString());
            var version=detail.GetProperty("version").GetBytesFromBase64();
            var identity=new ReviewAdmissionCommand(id,"Identity","Verified",31,null,version);
            await Expect(await client.PostAsJsonAsync(admission+"/Review",identity),HttpStatusCode.NoContent);
            await Expect(await client.PostAsJsonAsync(admission+"/Review",identity),HttpStatusCode.BadRequest);
            detail=await Json(await client.GetAsync(admission+"/"+id));
            Assert.Equal("Verified",detail.GetProperty("identityStatus").GetString());
            await Expect(await client.PostAsJsonAsync(admission+"/Review",new ReviewAdmissionCommand(id,"History","Reviewed",0,"Synthetic fixture",detail.GetProperty("version").GetBytesFromBase64())),HttpStatusCode.NoContent);
            await Expect(await client.PostAsJsonAsync(admission,input),HttpStatusCode.BadRequest);
            Actor("hemo.nurse");
            var requestInput=new RequestInput(id,Guid.NewGuid(),"General","Routine",new(){new("RBC",1)},new(ClinicalIndication:"Synthetic fixture only"));
            var req=(await Json(await client.PostAsJsonAsync(requests+"/Nurse",requestInput),HttpStatusCode.Created)).GetGuid();
            var draft=await Json(await client.GetAsync(requests+"/"+req));
            await Expect(await client.PostAsJsonAsync(requests+"/Submit",new DecisionInput(req,draft.GetProperty("version").GetBytesFromBase64(),"Submit")),HttpStatusCode.NoContent);
            Actor("hemo.doctor");
            var submitted=await Json(await client.GetAsync(requests+"/"+req));
            await Expect(await client.PostAsJsonAsync(requests+"/Decide",new DecisionInput(req,submitted.GetProperty("version").GetBytesFromBase64(),"Approve",null,new("Fixture","Fixture","FIX-ONLY",null,true))),HttpStatusCode.NoContent);
            Actor("hemo.bloodbank");
            var bankRequest=await Json(await client.GetAsync(requests+"/Bank/"+req));
            Assert.Equal("AwaitingBank",bankRequest.GetProperty("request").GetProperty("status").GetString());

            // R03-02: continue the SAME request through the real equipment, bank and transfusion endpoints.
            // All measurements and labels below are synthetic fixtures, not clinical recommendations.
            const string equipmentApi="/Identity/HemoQuality",bankApi="/Identity/BloodBank",transferApi="/Identity/Transfusion";
            var jsonOptions=new JsonSerializerOptions(JsonSerializerDefaults.Web);
            T Read<T>(JsonElement value)=>value.Deserialize<T>(jsonOptions)!;
            Actor("hemo.admission");
            var active=await Json(await client.GetAsync(admission+"/"+id));
            var prematureClearance=new ReviewAdmissionCommand(id,"Clearance","Ready",31,"Synthetic fixture",active.GetProperty("version").GetBytesFromBase64());
            await Expect(await client.PostAsJsonAsync(admission+"/Review",prematureClearance),HttpStatusCode.BadRequest);
            Assert.Equal("Active",(await Json(await client.GetAsync(admission+"/"+id))).GetProperty("status").GetString());

            const string equipmentCode="FIX-HTTP-FRIDGE",unitCode="FIX-HTTP-U1";
            Actor("hemo.equipment.tech");
            var equipment=(await Json(await client.PostAsJsonAsync(equipmentApi+"/EquipmentCreate",
                new EquipmentInput(equipmentCode,"Synthetic fridge","Refrigerator","Fixture model","Fixture ward")))).GetGuid();
            async Task<EquipmentDetail> Equipment()=>Read<EquipmentDetail>(await Json(await client.GetAsync(equipmentApi+"/EquipmentDetail/"+equipment)));
            var e=await Equipment();
            Assert.Equal("Hold",e.Equipment.Status);
            var now=DateTimeOffset.UtcNow;
            var receive=new ReceiveUnit(unitCode,"RBC","O+",null,"Synthetic supplier",equipmentCode,now.AddMinutes(-1),now.AddDays(1));
            Actor("hemo.bloodbank");
            await Expect(await client.PostAsJsonAsync(bankApi+"/Receive",receive),HttpStatusCode.BadRequest);
            Actor("hemo.equipment.reviewer");
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/EquipmentRelease",new EquipmentActionInput(equipment,e.Equipment.Version,"Synthetic premature release","FIX")),HttpStatusCode.BadRequest);
            Actor("hemo.equipment.tech");
            e=await Equipment();
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/EquipmentService",new ServiceInput(equipment,e.Equipment.Version,"Calibration",DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddDays(1),"FIX","Synthetic provider","Accepted","Synthetic checks","Synthetic only")),HttpStatusCode.NoContent);
            Actor("hemo.equipment.reviewer");e=await Equipment();
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/EquipmentRelease",new EquipmentActionInput(equipment,e.Equipment.Version,"Synthetic independent review","FIX")),HttpStatusCode.NoContent);
            Assert.Equal("Operational",(await Equipment()).Equipment.Status);
            Actor("hemo.equipment.tech");e=await Equipment();
            Assert.NotEqual(Guid.Empty,(await Json(await client.PostAsJsonAsync(equipmentApi+"/EquipmentTemperature",new EquipmentTemperatureInput(equipment,e.Equipment.Version,DateTimeOffset.UtcNow,"Morning",0m,-1m,1m,true,true,true,"Acceptable","Synthetic observation")))).GetGuid());

            Actor("hemo.bloodbank");
            var unit=(await Json(await client.PostAsJsonAsync(bankApi+"/Receive",receive),HttpStatusCode.Created)).GetGuid();
            await Expect(await client.PostAsJsonAsync(bankApi+"/Receive",receive),HttpStatusCode.BadRequest);
            async Task<UnitDetail> Unit()=>Read<UnitDetail>(await Json(await client.GetAsync(bankApi+"/Unit/"+unit)));
            async Task BankAction(string action,Guid? request=null,byte[]? requestVersion=null,HttpStatusCode expected=HttpStatusCode.NoContent)
            {
                var snapshot=await Unit();
                await Expect(await client.PostAsJsonAsync(bankApi+"/"+action,new UnitAction(unit,snapshot.Unit.Version,action,request,requestVersion,"Synthetic action")),expected);
            }
            Assert.Equal("ReceivedHold",(await Unit()).Unit.Status);
            var requestVersion=bankRequest.GetProperty("version").GetBytesFromBase64();
            await BankAction("Reserve",req,requestVersion,HttpStatusCode.BadRequest);
            Actor("hemo.bank.supervisor");await BankAction("Inspect");
            Assert.Equal("Available",(await Unit()).Unit.Status);
            Actor("hemo.bloodbank");
            bankRequest=await Json(await client.GetAsync(requests+"/Bank/"+req));
            requestVersion=bankRequest.GetProperty("version").GetBytesFromBase64();
            await BankAction("Reserve",req,requestVersion,HttpStatusCode.BadRequest);
            var labUnitSnapshot=await Unit();
            var lab=new LabInput(unit,labUnitSnapshot.Unit.Version,req,requestVersion,"FIX","Confirmed","O+","Synthetic method","NonReactive","NonReactive","NonReactive","NonReactive","NonReactive","FIX-REPORT","Accepted","Synthetic fixture");
            Assert.NotEqual(Guid.Empty,(await Json(await client.PostAsJsonAsync(bankApi+"/Test",lab))).GetGuid());
            await Expect(await client.PostAsJsonAsync(bankApi+"/Test",lab),HttpStatusCode.BadRequest);
            await BankAction("Reserve",req,requestVersion);Assert.Equal("Reserved",(await Unit()).Unit.Status);
            Actor("hemo.bank.supervisor");
            var candidates=await Json(await client.PostAsJsonAsync(transferApi+"/Candidates",new DispatchCandidatesQuery(RequestGuid:req)));
            Assert.Empty(candidates.GetProperty("data").EnumerateArray());
            Actor("hemo.bloodbank");await BankAction("Ready");Assert.Equal("Ready",(await Unit()).Unit.Status);
            Actor("hemo.bank.supervisor");
            candidates=await Json(await client.PostAsJsonAsync(transferApi+"/Candidates",new DispatchCandidatesQuery(RequestGuid:req)));
            var candidate=Read<DispatchCandidate>(Assert.Single(candidates.GetProperty("data").EnumerateArray()));
            Assert.Equal(unit,candidate.UnitGuid);Assert.Equal(req,candidate.RequestGuid);
            var dispatch=new DispatchInput(candidate.UnitGuid,candidate.UnitVersion,candidate.UnitCode,candidate.WristbandCode,"Synthetic courier","FIX","FIX-BOX",0m,"Synthetic indicator",null,31,"Synthetic fixture");
            await Expect(await client.PostAsJsonAsync(transferApi+"/Dispatch",dispatch with{WristbandCode="FIX-WRONG"}),HttpStatusCode.BadRequest);
            var transfer=(await Json(await client.PostAsJsonAsync(transferApi+"/Dispatch",dispatch),HttpStatusCode.Created)).GetGuid();
            await Expect(await client.PostAsJsonAsync(transferApi+"/Dispatch",dispatch),HttpStatusCode.BadRequest);
            Assert.Equal("Issued",(await Unit()).Unit.Status);
            candidates=await Json(await client.PostAsJsonAsync(transferApi+"/Candidates",new DispatchCandidatesQuery(RequestGuid:req)));
            Assert.Empty(candidates.GetProperty("data").EnumerateArray());
            async Task<TransferDetail> Transfer()=>Read<TransferDetail>(await Json(await client.GetAsync(transferApi+"/Detail/"+transfer)));
            async Task TransferAction(string action,HttpStatusCode expected=HttpStatusCode.NoContent,Func<TransferActionInput,TransferActionInput>? edit=null)
            {
                var t=await Transfer();
                var payload=new TransferActionInput(transfer,t.Transfer.Version,t.Transfer.UnitCode,t.WristbandCode,31,"Synthetic fixture",0m,"Acceptable","Synthetic receiver","Synthetic equipment");
                await Expect(await client.PostAsJsonAsync(transferApi+"/"+action,edit==null?payload:edit(payload)),expected);
            }
            async Task Vital(string phase)
            {
                var t=await Transfer();
                var payload=new VitalInput(transfer,t.Transfer.Version,phase,DateTimeOffset.UtcNow,120,80,75,37m,98,16,null,"Synthetic observation",10m);
                Assert.NotEqual(Guid.Empty,(await Json(await client.PostAsJsonAsync(transferApi+"/Vital",payload))).GetGuid());
            }
            Actor("hemo.nurse");
            Assert.Equal("Dispatched",(await Transfer()).Transfer.Status);
            await TransferAction("Receive");
            await TransferAction("Verify",HttpStatusCode.BadRequest,x=>x with{WristbandCode="FIX-WRONG"});
            Assert.Equal("Received",(await Transfer()).Transfer.Status);
            await TransferAction("Verify");
            await TransferAction("Witness",HttpStatusCode.BadRequest);
            Assert.Equal("Verified",(await Transfer()).Transfer.Status);
            Actor("hemo.nurse.witness");await TransferAction("Witness");
            Actor("hemo.nurse");
            var witnessed=await Transfer();Assert.Equal("Witnessed",witnessed.Transfer.Status);
            Assert.Equal(actors["hemo.nurse"].User,witnessed.VerifiedBy);
            Assert.Equal(actors["hemo.nurse.witness"].User,witnessed.WitnessedBy);
            await TransferAction("Start",HttpStatusCode.BadRequest);
            await Vital("T0");await TransferAction("Start");
            await TransferAction("Complete",HttpStatusCode.BadRequest,x=>x with{VolumeMl=100m});
            Assert.Equal("InProgress",(await Transfer()).Transfer.Status);
            await Vital("T15");await Vital("End");await TransferAction("Complete",edit:x=>x with{VolumeMl=100m});
            var completed=await Transfer();
            Assert.Equal("Completed",completed.Transfer.Status);
            Assert.Equal(id,completed.Transfer.AdmissionGuid);
            Assert.Equal(req,completed.Transfer.RequestGuid);
            Assert.Equal(unit,completed.Transfer.UnitGuid);
            Assert.Equal(100m,completed.Transfer.VolumeMl);
            Assert.Equal(new[]{"T0","T15","End"},completed.Vitals.Select(x=>x.Phase));
            Assert.Equal(new[]{"Dispatch","Receive","Verify","Witness","Start","Complete"},completed.Events.Select(x=>x.Action).Where(x=>x!="Vital"));
            Actor("hemo.bank.supervisor");
            Assert.Equal("Transfused",(await Unit()).Unit.Status);

            // R03-03: reaction and quality acceptance using the same persisted transfer and seeded roles.
            const string hvApi="/Identity/Hemovigilance";
            Actor("hemo.nurse");
            var reactionInput=new ReactionInput(transfer,completed.Transfer.Version,Guid.NewGuid(),DateTimeOffset.UtcNow,new[]{"Unknown"},"Unknown","Synthetic observation","Synthetic record");
            await Expect(await client.PostAsJsonAsync(hvApi+"/Report",reactionInput with{OnsetAt=DateTimeOffset.UtcNow.AddDays(-1)}),HttpStatusCode.BadRequest);
            var reaction=(await Json(await client.PostAsJsonAsync(hvApi+"/Report",reactionInput),HttpStatusCode.Created)).GetGuid();
            Assert.Equal(reaction,(await Json(await client.PostAsJsonAsync(hvApi+"/Report",reactionInput),HttpStatusCode.Created)).GetGuid());
            async Task<HvDetail> Reaction()=>Read<HvDetail>(await Json(await client.GetAsync(hvApi+"/Detail/"+reaction)));
            async Task HvReview(string action,HttpStatusCode expected=HttpStatusCode.NoContent,Func<ReviewInput,ReviewInput>? edit=null)
            {
                var current=await Reaction();var payload=new ReviewInput(reaction,current.Case.Version,"Synthetic fixture");
                await Expect(await client.PostAsJsonAsync(hvApi+"/"+action,edit==null?payload:edit(payload)),expected);
            }
            var initialReaction=await Reaction();
            Assert.Equal(transfer,initialReaction.Case.TransferGuid);
            Assert.Equal("Completed",initialReaction.TransferStatus);
            Assert.Equal(actors["hemo.nurse"].User,initialReaction.ReporterUserGuid);
            await HvReview("Observe");
            await Expect(await client.PostAsJsonAsync(hvApi+"/Observe",new ReviewInput(reaction,initialReaction.Case.Version,"Stale observation")),HttpStatusCode.BadRequest);
            await HvReview("DoctorReview",HttpStatusCode.Forbidden);
            Actor("hemo.bloodbank");await HvReview("BankReview",HttpStatusCode.BadRequest);
            Actor("hemo.doctor");await HvReview("DoctorReview",edit:x=>x with{DiagnosisCode="Undetermined",Severity="Unknown",Imputability="Unknown",Outcome="Unknown",Actions="Synthetic action",DoctorReference="FIX"});
            Actor("hemo.bloodbank");await HvReview("BankReview",edit:x=>x with{SampleReference="FIX",ReportReference="FIX",Disposition="Synthetic disposition"});
            Actor("hemo.hv.officer");await HvReview("ReturnToDoctor");
            Assert.Null((await Reaction()).Case.DiagnosisCode);
            Actor("hemo.doctor");await HvReview("DoctorReview",edit:x=>x with{DiagnosisCode="Undetermined",Severity="Unknown",Imputability="Unknown",Outcome="Unknown",Actions="Synthetic action",DoctorReference="FIX"});
            Actor("hemo.bloodbank");await HvReview("BankReview",edit:x=>x with{SampleReference="FIX",ReportReference="FIX",Disposition="Synthetic disposition"});
            Actor("hemo.hv.officer");await HvReview("Investigate",edit:x=>x with{RcaRequired=true,ExternalStatus="Required"});
            ReviewInput Rca(ReviewInput x)=>x with{RcaLead="Fixture",RcaMethod="FiveWhys",MeetingAt=DateTimeOffset.UtcNow,Participants="Fixture",DirectCause="Fixture",RootCause="Fixture"};
            await HvReview("PublishRca",HttpStatusCode.BadRequest,Rca);
            var hvOwners=Read<List<HvOwner>>(await Json(await client.GetAsync(hvApi+"/Owners")));
            var nurseGuid=actors["hemo.nurse"].User;
            Assert.Contains(hvOwners,x=>x.UserGuid==nurseGuid);
            var capa=(await Json(await client.PostAsJsonAsync(hvApi+"/AssignCapa",new CapaCreateInput(reaction,(await Reaction()).Case.Version,"Fixture corrective action","Corrective",nurseGuid,DateTimeOffset.UtcNow.AddDays(1),"Fixture indicator")),HttpStatusCode.Created)).GetGuid();
            await HvReview("PublishRca",edit:Rca);
            Actor("hemo.hv.reviewer");await HvReview("Close",HttpStatusCode.BadRequest);
            async Task CapaAction(string action,int progress,HttpStatusCode expected=HttpStatusCode.NoContent)
            {
                var current=await Reaction();var line=current.Actions.Single(x=>x.Guid==capa);
                await Expect(await client.PostAsJsonAsync(hvApi+"/"+action,new CapaActionInput(capa,line.Version,current.Case.Version,progress,"Synthetic evidence")),expected);
            }
            Actor("hemo.doctor");await CapaAction("CapaProgress",100,HttpStatusCode.BadRequest);
            Actor("hemo.nurse");await CapaAction("CapaProgress",40);await CapaAction("CapaProgress",100);
            await CapaAction("CapaVerify",100,HttpStatusCode.Forbidden);
            Actor("hemo.hv.reviewer");await CapaAction("CapaReject",100);
            Actor("hemo.nurse");await CapaAction("CapaProgress",100);
            Actor("hemo.hv.reviewer");await CapaAction("CapaVerify",100);await HvReview("Close",HttpStatusCode.BadRequest);
            Actor("hemo.hv.officer");
            await HvReview("External",HttpStatusCode.BadRequest,x=>x with{ExternalStatus="SentManually"});
            await HvReview("External",edit:x=>x with{ExternalStatus="SentManually",ExternalReference="FIX-EXTERNAL"});
            var report=(await Json(await client.PostAsJsonAsync(hvApi+"/Archive",new ReportInput("Synthetic report",DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow,"Fixture")),HttpStatusCode.Created)).GetGuid();
            async Task<HvReportDetail> Archived()=>Read<HvReportDetail>(await Json(await client.GetAsync(hvApi+"/ReportDetail/"+report)));
            var archivedBefore=await Archived();
            Assert.Equal(1,archivedBefore.Summary.Total);Assert.Equal(0,archivedBefore.Summary.Closed);
            await Expect(await client.PostAsJsonAsync(hvApi+"/ReportExternal",new ReportExternalInput(report,archivedBefore.Report.Version,"SentManually","FIX-REPORT")),HttpStatusCode.NoContent);
            Actor("hemo.hv.reviewer");await HvReview("Close");
            var closedReaction=await Reaction();
            Assert.Equal("Closed",closedReaction.Case.Status);Assert.Equal("Verified",Assert.Single(closedReaction.Actions).Status);
            Assert.Contains(closedReaction.Events,x=>x.Action=="ReturnToDoctor");
            Assert.Equal(0,(await Archived()).Summary.Closed);
            var liveHv=Read<HvSummary>(await Json(await client.PostAsJsonAsync(hvApi+"/Stats",new HvFilters(Ward:"Fixture"))));
            Assert.Equal(1,liveHv.Closed);
            Actor("hemo.nurse");await HvReview("Observe",HttpStatusCode.BadRequest);
            Actor("hemo.admission");var finalAdmission=await Json(await client.GetAsync(admission+"/"+id));
            await Expect(await client.PostAsJsonAsync(admission+"/Review",new ReviewAdmissionCommand(id,"Clearance","Ready",31,"Synthetic documented review",finalAdmission.GetProperty("version").GetBytesFromBase64())),HttpStatusCode.NoContent);
            Assert.Equal("ReadyForDischarge",(await Json(await client.GetAsync(admission+"/"+id))).GetProperty("status").GetString());
            Actor("hemo.nurse");await Expect(await client.PostAsJsonAsync(requests+"/Nurse",requestInput with{ClientRequestId=Guid.NewGuid()}),HttpStatusCode.BadRequest);

            // Exercise the existing R02 quality fixtures, not fabricated statuses or direct handler calls.
            async Task<QualityDetail> Quality(string kind,Guid recordId)=>Read<QualityDetail>(await Json(await client.GetAsync(equipmentApi+"/"+kind+"Detail/"+recordId)));
            async Task<QualityRow> SingleQuality(string kind,QualityFilter? filter=null)
            {
                var page=await Json(await client.PostAsJsonAsync(equipmentApi+"/"+kind+"List",filter??new QualityFilter()));
                return Read<QualityRow>(Assert.Single(page.GetProperty("data").EnumerateArray()));
            }
            async Task QualityReview(string kind,Guid recordId,string action,HttpStatusCode expected=HttpStatusCode.NoContent)
            {
                var current=await Quality(kind,recordId);
                await Expect(await client.PostAsJsonAsync(equipmentApi+"/"+kind+action,new QualityAction(recordId,current.Record.Version,"Fixture review")),expected);
            }
            Actor("hemo.quality.officer");
            var auditRow=await SingleQuality("Audit");var auditDetail=await Quality("Audit",auditRow.Guid);
            Assert.Equal(1,auditDetail.AuditScore);Assert.Equal(2,auditDetail.AuditMaximum);
            Assert.NotEqual(Guid.Empty,(await Json(await client.PostAsJsonAsync(equipmentApi+"/AuditLine",new QualityLineInput(auditRow.Guid,auditDetail.Record.Version,"Fixture clause",new QualityLineData(Clause:"FIX-2",Score:2))))).GetGuid());
            await QualityReview("Audit",auditRow.Guid,"Submit");
            await QualityReview("Audit",auditRow.Guid,"Approve",HttpStatusCode.Forbidden);
            Actor("hemo.quality.reviewer");await QualityReview("Audit",auditRow.Guid,"Approve");
            auditDetail=await Quality("Audit",auditRow.Guid);Assert.Equal("Approved",auditDetail.Record.Status);Assert.Equal(3,auditDetail.AuditScore);Assert.Equal(4,auditDetail.AuditMaximum);
            var qcRow=await SingleQuality("Qc");await QualityReview("Qc",qcRow.Guid,"Reject");
            // A rejected QC is returned for correction; re-submit with the authorized officer.
            Actor("hemo.quality.officer");await QualityReview("Qc",qcRow.Guid,"Submit");
            Actor("hemo.quality.reviewer");await QualityReview("Qc",qcRow.Guid,"Approve");
            Assert.Equal("Approved",(await Quality("Qc",qcRow.Guid)).Record.Status);
            Actor("hemo.quality.officer");
            var trainingRow=await SingleQuality("Training");await QualityReview("Training",trainingRow.Guid,"Submit",HttpStatusCode.BadRequest);
            var trainingDetail=await Quality("Training",trainingRow.Guid);var attendance=Assert.Single(trainingDetail.Lines);
            var trainingOwners=Read<List<QualityOwner>>(await Json(await client.GetAsync(equipmentApi+"/TrainingOwners")));
            Assert.Contains(trainingOwners,x=>x.UserGuid==attendance.OwnerUserGuid);
            await Json(await client.PostAsJsonAsync(equipmentApi+"/TrainingLine",new QualityLineInput(trainingRow.Guid,trainingDetail.Record.Version,attendance.Title,attendance.Data with{Attendance="Present",PreScore=10,PostScore=15,CertificateReference="FIX"},attendance.OwnerUserGuid,null,attendance.Guid,attendance.Version)));
            await QualityReview("Training",trainingRow.Guid,"Submit");
            Actor("hemo.quality.reviewer");await QualityReview("Training",trainingRow.Guid,"Approve");
            Assert.Equal("Approved",(await Quality("Training",trainingRow.Guid)).Record.Status);
            Actor("hemo.nurse");var meetingRow=await SingleQuality("Committee",new QualityFilter(Mine:true));
            async Task CommitteeAction(string action,int progress,HttpStatusCode expected=HttpStatusCode.NoContent)
            {
                var current=await Quality("Committee",meetingRow.Guid);var resolution=Assert.Single(current.Lines);
                await Expect(await client.PostAsJsonAsync(equipmentApi+"/Committee"+action,new ResolutionProgress(meetingRow.Guid,current.Record.Version,resolution.Guid,resolution.Version,progress,"Fixture evidence")),expected);
            }
            await CommitteeAction("Progress",100);await CommitteeAction("Verify",100,HttpStatusCode.Forbidden);
            Actor("hemo.quality.reviewer");await QualityReview("Committee",meetingRow.Guid,"Close",HttpStatusCode.BadRequest);
            await CommitteeAction("Return",100);
            Actor("hemo.nurse");await CommitteeAction("Progress",100);
            Actor("hemo.quality.reviewer");await CommitteeAction("Verify",100);await QualityReview("Committee",meetingRow.Guid,"Close");
            Assert.Equal("Closed",(await Quality("Committee",meetingRow.Guid)).Record.Status);
            var qualityStats=Read<QualityStats>(await Json(await client.PostAsJsonAsync(equipmentApi+"/Stats",new QualityStatsFilter())));
            Assert.Equal(1,qualityStats.Present);Assert.Equal(0,qualityStats.OpenResolutions);
            Actor("hemo.quality.officer");
            var facilityBefore=Read<FacilityView>(await Json(await client.GetAsync(equipmentApi+"/Facility")));
            var facilityUpdate=new FacilityInput(facilityBefore.Version,"Synthetic updated",null,null,null,null,null,null,"Fixture","Fixture change");
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/FacilitySave",facilityUpdate),HttpStatusCode.NoContent);
            var facilityAfter=Read<FacilityView>(await Json(await client.GetAsync(equipmentApi+"/Facility")));
            Assert.Equal("Synthetic updated",facilityAfter.FacilityName);Assert.Contains(facilityAfter.History!,x=>x.Notes=="Fixture change");
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/FacilitySave",facilityUpdate),HttpStatusCode.BadRequest);

            Actor("hemo.equipment.tech");e=await Equipment();
            var incident=(await Json(await client.PostAsJsonAsync(equipmentApi+"/IncidentReport",new IncidentInput(equipment,e.Equipment.Version,"High","Synthetic incident")))).GetGuid();
            e=await Equipment();Assert.Equal("Hold",e.Equipment.Status);
            var incidentRow=e.Incidents.Single(x=>x.Guid==incident);
            Actor("hemo.equipment.reviewer");
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/EquipmentRelease",new EquipmentActionInput(equipment,e.Equipment.Version,"Fixture premature review","FIX")),HttpStatusCode.BadRequest);
            Actor("hemo.equipment.tech");
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/IncidentResolve",new IncidentResolve(incident,incidentRow.Version,e.Equipment.Version,"Synthetic resolution","FIX")),HttpStatusCode.NoContent);
            e=await Equipment();Assert.Equal("Resolved",e.Incidents.Single(x=>x.Guid==incident).Status);
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/EquipmentService",new ServiceInput(equipment,e.Equipment.Version,"Calibration",DateTimeOffset.UtcNow,DateTimeOffset.UtcNow.AddDays(1),"FIX","Synthetic provider","Accepted","Synthetic checks","Synthetic only")),HttpStatusCode.NoContent);
            Actor("hemo.equipment.reviewer");e=await Equipment();
            await Expect(await client.PostAsJsonAsync(equipmentApi+"/EquipmentRelease",new EquipmentActionInput(equipment,e.Equipment.Version,"Fixture independent review","FIX")),HttpStatusCode.NoContent);
            Assert.Equal("Operational",(await Equipment()).Equipment.Status);
            Actor(manager);
            await Expect(await client.GetAsync(hvApi+"/Detail/"+reaction),HttpStatusCode.Forbidden);
            await Expect(await client.GetAsync(hvApi+"/ReportDetail/"+report),HttpStatusCode.Forbidden);
            await Expect(await client.GetAsync(equipmentApi+"/AuditDetail/"+auditRow.Guid),HttpStatusCode.Forbidden);
            Actor(manager);await Expect(await client.GetAsync(transferApi+"/Detail/"+transfer),HttpStatusCode.Forbidden);
            Actor(manager);await Expect(await client.GetAsync(admission+"/"+id),HttpStatusCode.Forbidden);
            await Expect(await client.GetAsync(requests+"/"+req),HttpStatusCode.Forbidden);
        }
        finally
        {
            if(started)await app.StopAsync();
            using var scope=app.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureDeletedAsync();
        }
    }
}
