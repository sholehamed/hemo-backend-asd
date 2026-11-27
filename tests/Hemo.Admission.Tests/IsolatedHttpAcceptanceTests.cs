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
using Application.Core.PatientAdmission_core.Commands.CreatePatientAdmission;
using Application.Core.PatientAdmission_core.Commands.ReviewAdmission;
using Application.Repository.Services;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using web.Infrastructure;
using Xunit;

namespace Hemo.Admission.Tests;

// Never runs company Program or its hard-coded database/external integrations.
// Real endpoint classes, application DI, JWT bearer validation, token store and SQL.
public class IsolatedHttpAcceptanceTests
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
        app.UseAuthentication();
        app.UseAuthorization();
        new Web.Endpoints.PatientAdmission().Map(app);
        new Web.Endpoints.BloodRequest().Map(app);
        new Web.Endpoints.BloodBank().Map(app);
        new Web.Endpoints.Transfusion().Map(app);
        new Web.Endpoints.Hemovigilance().Map(app);
        new Web.Endpoints.HemoQuality().Map(app);
        new Web.Endpoints.RoleAccess().Map(app);
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
