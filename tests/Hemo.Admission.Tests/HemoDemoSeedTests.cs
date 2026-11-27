using Application.Common.Utility.Security;
using Application.Core.User_core.Commands.UserLogin;
using Application.Repository.Services;
using Domain.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hemo.Admission.Tests;
public class HemoDemoSeedTests
{
    private sealed class Audit:SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e,InterceptionResult<int> result,CancellationToken ct=default)
        {
            foreach(var row in e.Context!.ChangeTracker.Entries<BaseEntity>())
            {
                if(row.State==EntityState.Added)row.Entity.Created=DateTime.UtcNow;
                if(row.Entity is AuditedEntity audited && row.State is EntityState.Added or EntityState.Modified)
                {
                    audited.CreatedByIP="test";audited.LastModifiedByIP="test";audited.LastModified=DateTime.UtcNow;
                }
            }
            return ValueTask.FromResult(result);
        }
    }
    [Fact]
    [Trait("Category","SqlServer")]
    public async Task SeedIsRepeatableAndRolesAreEligibleWithoutGlobalPrivileges()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_seed_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);
        var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,true,true,true);
            var secrets=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeHemovigilance"]="true";
            var configuration=new ConfigurationBuilder().AddInMemoryCollection(secrets).Build();
            await HemoDemoSeed.ApplyAsync(db,configuration,ct);
            var userCount=await db.User.CountAsync(ct);
            var roleCount=await db.Role.CountAsync(ct);
            var grantCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"HemoSeed:IncludeHemovigilance","true"}}).Build(),ct);
            Assert.Equal(userCount,await db.User.CountAsync(ct));
            Assert.Equal(roleCount,await db.Role.CountAsync(ct));
            Assert.Equal(grantCount,await db.RoleAccess.CountAsync(ct));
            foreach(var account in accounts)
            {
                var user=await db.User.SingleAsync(u=>u.Username==account.Username,ct);
                Assert.Equal(UserService.HashPassword(user.GUID,secrets["HemoSeed:Passwords:"+account.Key]!),user.Password);
                var assignment=Assert.Single(await HemoLoginContext.EligibleRoles(db,user.GUID,DateTime.Now).ToListAsync(ct));
                Assert.True(assignment.Role.Gkey>2);
                var permissions=await db.RoleAccess.Where(r=>r.RoleGUID==assignment.RoleGUID).Select(r=>r.ApiGUID).ToListAsync(ct);
                Assert.Equal(account.Permissions.Length,permissions.Count);
                Assert.DoesNotContain(permissions,p=>p==null);
            }
        }
        finally {await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    public void AdmissionRoutePolicyRequiresExistingEndpointPermissions()
    {
        Assert.Equal(8,HemoAdmissionRouteSeed.Routes.Length);
        Assert.Equal(8,HemoAdmissionRouteSeed.Routes.Select(r=>r.Path).Distinct().Count());
        var keys=HemoDemoSeed.Endpoints.Select(e=>e.Key).ToHashSet();
        foreach(var route in HemoAdmissionRouteSeed.Routes)
        {
            Assert.All(route.Required,key=>Assert.Contains(key,keys));
            Assert.All(route.Optional,key=>Assert.Contains(key,keys));
            Assert.NotEmpty(route.Required);
            Assert.Equal(route.Path.Contains(':'),!route.ShowInMenu);
        }
        var manager=HemoDemoSeed.DashboardManager.Permissions;
        Assert.Equal("/admission/stats",Assert.Single(HemoAdmissionRouteSeed.Routes.Where(r=>HemoAdmissionRouteSeed.CanOpen(r,manager))).Path);
        Assert.True(HemoAdmissionRouteSeed.CanOpen(HemoAdmissionRouteSeed.DashboardRoute,manager));
        Assert.True(HemoAdmissionRouteSeed.CanOpen(HemoAdmissionRouteSeed.DashboardRoute,new[]{"list"}));
        Assert.False(HemoAdmissionRouteSeed.CanOpen(HemoAdmissionRouteSeed.DashboardRoute,new[]{"detail"}));
        Assert.False(HemoAdmissionRouteSeed.CanOpen(HemoAdmissionRouteSeed.DashboardRoute,Array.Empty<string>()));
        var viewer=HemoDemoSeed.Accounts.Single(a=>a.Key=="Viewer").Permissions;
        Assert.DoesNotContain(HemoAdmissionRouteSeed.Routes.Where(r=>HemoAdmissionRouteSeed.CanOpen(r,viewer)),r=>r.Path=="/admission/new");
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task AdmissionRouteSeedIsOptInAndDoesNotExpandEndpointGrants()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_R02_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,false);
            var secrets=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeBloodRequests"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(secrets).Build(),ct);
            Assert.False(await db.Menu.AnyAsync(m=>m.routerLink=="/admission/new",ct));
            var before=(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            var users=await db.User.CountAsync(ct);
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeAdmissionRouteMenus"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            var after=(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            Assert.True(before.SetEquals(after));
            Assert.Equal(users,await db.User.CountAsync(ct));
            var count=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(count,await db.RoleAccess.CountAsync(ct));
            var menu=await db.Menu.SingleAsync(m=>m.routerLink=="/admission/detail/:guid",ct);
            Assert.False(menu.ShowInMenu);Assert.False(menu.IsPublic);
            var viewer=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:Viewer",ct);
            var admission=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:Admission",ct);
            var nurse=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:Nurse",ct);
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.RoleGUID==viewer.GUID&&r.Menu.routerLink=="/admission/new",ct));
            Assert.True(await db.RoleAccess.AnyAsync(r=>r.RoleGUID==admission.GUID&&r.Menu.routerLink=="/admission/new",ct));
            Assert.True(await db.RoleAccess.AnyAsync(r=>r.RoleGUID==nurse.GUID&&r.MenuGUID==menu.GUID,ct));
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.RoleGUID==nurse.GUID&&r.Menu.routerLink=="/admission/discharge",ct));
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.Menu.ConvertID!=null&&r.Menu.ConvertID.StartsWith("HEMO-R02-ADMISSION:")&&r.ApiGUID==null,ct));
        }
        finally {await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task CompanyMenuPilotExposesOnlyActiveReportUnderHemoRoot()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_Menu_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,false);
            var secrets=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeBloodRequests"]="true";
            secrets["HemoSeed:IncludeAdmissionRouteMenus"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(secrets).Build(),ct);
            var stats=await db.Menu.SingleAsync(m=>m.routerLink=="/admission/stats",ct);
            var statsId=stats.GUID;
            Assert.Null(stats.PMenuGUID);
            var grants=(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.MenuGUID,r.ApiGUID}).ToListAsync(ct)).ToHashSet();
            var mappings=await db.MenuApi.CountAsync(ct);
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeCompanyMenuPilot"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            db.ChangeTracker.Clear();
            var root=await db.Menu.SingleAsync(m=>m.routerLink=="/Hemo",ct);
            Assert.Null(root.PMenuGUID);Assert.False(root.IsPublic);Assert.True(root.ShowInMenu);
            var child=Assert.Single(await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct));
            Assert.Equal(statsId,child.GUID);Assert.Equal("/admission/stats",child.routerLink);
            Assert.Equal(root.Path+child.Gkey+"/",child.Path);Assert.Equal(root.Level+1,child.Level);
            var viewer=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:Viewer",ct);
            var nurse=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:Nurse",ct);
            // The exact existing MenuBindUserMenu predicate: only ancestors of
            // visible granted children are visible, without a direct root grant.
            async Task<bool> RootVisible(Guid role)=>await db.Menu.AnyAsync(a=>a.GUID==root.GUID&&a.ShowInMenu&&
                (a.RoleAccess.Any(b=>b.RoleGUID==role)||db.Menu.Any(b=>b.Path.StartsWith(a.Path)&&b.ShowInMenu&&
                (b.RoleAccess.Any(c=>c.RoleGUID==role)||b.IsPublic))),ct);
            Assert.True(await RootVisible(viewer.GUID));Assert.False(await RootVisible(nurse.GUID));
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.MenuGUID==root.GUID,ct));
            Assert.False(await db.MenuApi.AnyAsync(r=>r.MenuGUID==root.GUID,ct));
            Assert.True(grants.SetEquals(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.MenuGUID,r.ApiGUID}).ToListAsync(ct)));
            Assert.Equal(mappings,await db.MenuApi.CountAsync(ct));
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(1,await db.Menu.CountAsync(m=>m.routerLink=="/Hemo",ct));
            Assert.True(grants.SetEquals(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.MenuGUID,r.ApiGUID}).ToListAsync(ct)));
            // Re-running with the previous admission-only flag must not detach the leaf.
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeAdmissionRouteMenus"]="true"
            }).Build(),ct);
            Assert.Equal(root.GUID,(await db.Menu.SingleAsync(m=>m.GUID==statsId,ct)).PMenuGUID);

            // Upgrade the prior report-only deployment without adding API grants.
            var readPaths=new[]{"/admission/list","/admission/his"};
            var previous=await db.Menu.Where(m=>readPaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(2,previous.Count);
            Assert.All(previous,m=>Assert.Null(m.PMenuGUID));
            var readIds=previous.Select(m=>m.GUID).ToHashSet();
            var readCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeAdmissionReadPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,readCfg,ct);
            db.ChangeTracker.Clear();
            var attached=await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct);
            Assert.Equal(3,attached.Count);
            Assert.True(readIds.SetEquals(attached.Where(m=>readPaths.Contains(m.routerLink)).Select(m=>m.GUID)));
            Assert.All(attached,m=>{Assert.Equal(root.Path+m.Gkey+"/",m.Path);Assert.Equal(root.Level+1,m.Level);Assert.False(m.IsPublic);});
            Assert.True(await RootVisible(nurse.GUID));
            Assert.Equal(2,await db.RoleAccess.Where(r=>r.RoleGUID==nurse.GUID&&readPaths.Contains(r.Menu.routerLink)).Select(r=>r.MenuGUID).Distinct().CountAsync(ct));
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.RoleGUID==nurse.GUID&&r.MenuGUID==statsId,ct));
            Assert.True(grants.SetEquals(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.MenuGUID,r.ApiGUID}).ToListAsync(ct)));
            Assert.Equal(mappings,await db.MenuApi.CountAsync(ct));
            await HemoDemoSeed.ApplyAsync(db,readCfg,ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(3,await db.Menu.CountAsync(m=>m.PMenuGUID==root.GUID,ct));
            Assert.True(grants.SetEquals(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.MenuGUID,r.ApiGUID}).ToListAsync(ct)));

            var original=await db.Menu.SingleAsync(m=>m.routerLink=="/admission",ct);
            var originalId=original.GUID;var originalParent=original.PMenuGUID;
            var originalPaths=HemoAdmissionRouteSeed.Routes.Select(r=>r.Path).ToArray();
            var oldIds=(await db.Menu.Where(m=>originalPaths.Contains(m.routerLink)).Select(m=>m.GUID).ToListAsync(ct)).ToHashSet();
            var endpointGrants=(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            var completeCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeAdmissionCompletePages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,completeCfg,ct);
            db.ChangeTracker.Clear();
            var children=await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct);
            Assert.Equal(9,children.Count);
            Assert.Equal(8,children.Count(m=>m.ShowInMenu));
            Assert.True(oldIds.SetEquals(children.Where(m=>originalPaths.Contains(m.routerLink)).Select(m=>m.GUID)));
            Assert.All(children,m=>{Assert.False(m.IsPublic);Assert.Equal(root.Path+m.Gkey+"/",m.Path);});
            Assert.Equal(originalParent,(await db.Menu.SingleAsync(m=>m.GUID==originalId,ct)).PMenuGUID);
            var dashboard=Assert.Single(children.Where(m=>m.routerLink=="/admission/dashboard"));
            Assert.True(await db.RoleAccess.AnyAsync(r=>r.RoleGUID==nurse.GUID&&r.MenuGUID==dashboard.GUID,ct));
            var statsApiIds=await db.MenuApi.Where(m=>m.MenuGUID==statsId).Select(m=>m.ApiGUID).ToListAsync(ct);
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.RoleGUID==nurse.GUID&&r.MenuGUID==dashboard.GUID&&r.ApiGUID.HasValue&&statsApiIds.Contains(r.ApiGUID.Value),ct));
            Assert.True(grants.SetEquals(await db.RoleAccess.Where(r=>r.MenuGUID!=dashboard.GUID).Select(r=>new {r.RoleGUID,r.MenuGUID,r.ApiGUID}).ToListAsync(ct)));
            Assert.True(endpointGrants.SetEquals(await db.RoleAccess.Select(r=>new {r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.MenuGUID==dashboard.GUID&&r.ApiGUID==null,ct));
            var completeGrantCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,completeCfg,ct);
            await HemoDemoSeed.ApplyAsync(db,readCfg,ct);
            Assert.Equal(9,await db.Menu.CountAsync(m=>m.PMenuGUID==root.GUID,ct));
            Assert.Equal(completeGrantCount,await db.RoleAccess.CountAsync(ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    public void BloodRequestReadRoutePolicyNeverMapsMutation()
    {
        var routes=HemoRequestReadRouteSeed.Routes;
        Assert.Equal(3,routes.Length);
        var nurse=HemoDemoSeed.BloodRequestAccounts.Single(a=>a.Key=="Nurse").Permissions;
        var doctor=HemoDemoSeed.BloodRequestAccounts.Single(a=>a.Key=="Doctor").Permissions;
        var bank=HemoDemoSeed.BloodRequestAccounts.Single(a=>a.Key=="BloodBank").Permissions;
        Assert.Empty(routes.Where(r=>HemoRequestReadRouteSeed.CanOpen(r,nurse)));
        Assert.Equal(2,routes.Count(r=>HemoRequestReadRouteSeed.CanOpen(r,doctor)));
        Assert.Equal("/hemo/requests/bank",Assert.Single(routes.Where(r=>HemoRequestReadRouteSeed.CanOpen(r,bank))).Path);
        Assert.All(routes,r=>Assert.All(r.Mapped,k=>Assert.Contains(k,new[]{"request-list","request-bank"})));
        Assert.Empty(routes.Where(r=>HemoRequestReadRouteSeed.CanOpen(r,HemoDemoSeed.DashboardManager.Permissions)));
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task RequestReadMenuPreservesOriginalGrantsAndIsRepeatable()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_RequestMenu_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,false);
            var secrets=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            secrets["HemoSeed:IncludeBloodRequests"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(secrets).Build(),ct);
            var original=await db.Menu.SingleAsync(m=>m.routerLink=="/hemo/requests",ct);
            var originalId=original.GUID;
            Assert.Null(original.PMenuGUID);
            var paths=HemoRequestReadRouteSeed.Routes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>paths.Contains(m.routerLink),ct));
            var before=(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            var originalGrants=(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)).ToHashSet();
            var users=await db.User.CountAsync(ct);
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeRequestReadPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            db.ChangeTracker.Clear();
            var root=await db.Menu.SingleAsync(m=>m.routerLink=="/Hemo",ct);
            var children=await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct);
            Assert.Equal(4,children.Count);
            Assert.Equal(originalId,Assert.Single(children.Where(m=>m.routerLink=="/hemo/requests")).GUID);
            Assert.All(children,m=>{Assert.False(m.IsPublic);Assert.Equal(root.Path+m.Gkey+"/",m.Path);});
            Assert.False(Assert.Single(children.Where(m=>m.routerLink=="/hemo/requests/doctor")).ShowInMenu);
            Assert.Equal(users,await db.User.CountAsync(ct));
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            Assert.True(originalGrants.SetEquals(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)));
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.MenuGUID==root.GUID,ct));
            var roles=await db.Role.Where(r=>r.ConvertID!=null).ToListAsync(ct);
            foreach(var route in HemoRequestReadRouteSeed.Routes)
            {
                var leaf=Assert.Single(children.Where(m=>m.routerLink==route.Path));
                var grants=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var eligible=accounts.Where(a=>HemoRequestReadRouteSeed.CanOpen(route,a.Permissions)).ToArray();
                Assert.Equal(eligible.Length,grants.Count);
                foreach(var account in eligible)
                    Assert.Contains(grants,g=>g.RoleGUID==roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID&&g.ApiGUID.HasValue);
            }
            var count=await db.RoleAccess.CountAsync(ct);
            var mappings=await db.MenuApi.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(count,await db.RoleAccess.CountAsync(ct));Assert.Equal(mappings,await db.MenuApi.CountAsync(ct));
            var detailCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeRequestDetailPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,detailCfg,ct);
            db.ChangeTracker.Clear();
            var detailPaths=HemoRequestReadRouteSeed.DetailRoutes.Select(r=>r.Path).ToArray();
            var leaves=await db.Menu.Where(m=>detailPaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(2,leaves.Count);
            Assert.All(leaves,m=>{Assert.False(m.ShowInMenu);Assert.False(m.IsPublic);Assert.Equal(root.GUID,m.PMenuGUID);});
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            foreach(var route in HemoRequestReadRouteSeed.DetailRoutes)
            {
                var leaf=Assert.Single(leaves.Where(m=>m.routerLink==route.Path));
                var grants=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var expected=new HashSet<(Guid,Guid?)>();
                foreach(var account in accounts.Where(a=>HemoRequestReadRouteSeed.CanOpen(route,a.Permissions)))
                {
                    var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                    foreach(var key in route.Mapped.Where(k=>account.Permissions.Contains(k)))
                    {
                        var endpoint=HemoDemoSeed.BloodRequestEndpoints.Single(e=>e.Key==key);
                        var api=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                        expected.Add((roleId,api.GUID));
                    }
                }
                Assert.True(expected.SetEquals(grants.Select(g=>(g.RoleGUID,g.ApiGUID))));
            }
            var detailCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,detailCfg,ct);
            Assert.Equal(detailCount,await db.RoleAccess.CountAsync(ct));
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(detailCount,await db.RoleAccess.CountAsync(ct));
            var formPaths=HemoRequestReadRouteSeed.FormRoutes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>formPaths.Contains(m.routerLink),ct));
            var formCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodRequests"]="true",["HemoSeed:IncludeRequestFormPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,formCfg,ct);
            db.ChangeTracker.Clear();
            var formMenus=await db.Menu.Where(m=>formPaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(3,formMenus.Count);
            Assert.Equal(2,formMenus.Count(m=>m.ShowInMenu));
            Assert.All(formMenus,m=>{Assert.False(m.IsPublic);Assert.Equal(root.GUID,m.PMenuGUID);Assert.Equal(root.Path+m.Gkey+"/",m.Path);});
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            foreach(var route in HemoRequestReadRouteSeed.FormRoutes)
            {
                var leaf=Assert.Single(formMenus.Where(m=>m.routerLink==route.Path));
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var expected=new HashSet<(Guid,Guid?)>();
                foreach(var account in accounts.Where(a=>HemoRequestReadRouteSeed.CanOpen(route,a.Permissions)))
                {
                    var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                    foreach(var key in route.Mapped.Where(k=>account.Permissions.Contains(k)))
                    {
                        var endpoint=HemoDemoSeed.Endpoints.Concat(HemoDemoSeed.BloodRequestEndpoints).Single(e=>e.Key==key);
                        var api=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                        expected.Add((roleId,api.GUID));
                    }
                }
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
            }
            var formCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,formCfg,ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(formCount,await db.RoleAccess.CountAsync(ct));
            Assert.Equal(users,await db.User.CountAsync(ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    public void BloodRequestFormRoutePolicyRequiresBothAdmissionAndRequestPermissions()
    {
        var routes=HemoRequestReadRouteSeed.FormRoutes;
        var nurse=HemoDemoSeed.BloodRequestAccounts.Single(a=>a.Key=="Nurse").Permissions;
        var doctor=HemoDemoSeed.BloodRequestAccounts.Single(a=>a.Key=="Doctor").Permissions;
        var bank=HemoDemoSeed.BloodRequestAccounts.Single(a=>a.Key=="BloodBank").Permissions;
        Assert.Equal(2,routes.Count(r=>HemoRequestReadRouteSeed.CanOpen(r,nurse)));
        Assert.Equal(2,routes.Count(r=>HemoRequestReadRouteSeed.CanOpen(r,doctor)));
        Assert.Empty(routes.Where(r=>HemoRequestReadRouteSeed.CanOpen(r,bank)));
        Assert.Empty(routes.Where(r=>HemoRequestReadRouteSeed.CanOpen(r,nurse.Where(k=>k!="detail"))));
        Assert.DoesNotContain(routes.Where(r=>HemoRequestReadRouteSeed.CanOpen(r,nurse)),r=>r.Path=="/hemo/requests/orders");
        Assert.DoesNotContain(routes.Where(r=>HemoRequestReadRouteSeed.CanOpen(r,doctor)),r=>r.Path=="/hemo/requests/new");
        Assert.All(routes,r=>Assert.DoesNotContain("request-decide",r.Mapped));
    }

    [Fact]
    public void BankReadRoutePolicyDoesNotGrantClinicalOperations()
    {
        var routes=HemoBankReadRouteSeed.Routes;
        Assert.Equal(2,routes.Length);
        Assert.All(routes,r=>Assert.True(HemoBankReadRouteSeed.CanOpen(r,HemoDemoSeed.BankOperatorKeys)));
        Assert.All(routes,r=>Assert.True(HemoBankReadRouteSeed.CanOpen(r,HemoDemoSeed.BankSupervisorKeys)));
        Assert.Equal("/hemo/bank/reports",Assert.Single(routes.Where(r=>HemoBankReadRouteSeed.CanOpen(r,HemoDemoSeed.DashboardManager.Permissions))).Path);
        Assert.Empty(routes.Where(r=>HemoBankReadRouteSeed.CanOpen(r,HemoDemoSeed.BloodRequestAccounts.Single(a=>a.Key=="Nurse").Permissions)));
        Assert.All(routes,r=>Assert.All(r.Mapped,k=>Assert.Contains(k,HemoDemoSeed.BankReadKeys)));
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task BankReadMenusPreserveGrantsAndAreRepeatable()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_BankMenu_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,true);
            var settings=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            settings["HemoSeed:IncludeBloodBank"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(settings).Build(),ct);
            var original=await db.Menu.SingleAsync(m=>m.routerLink=="/hemo/bank",ct);var originalId=original.GUID;
            var paths=HemoBankReadRouteSeed.Routes.Select(r=>r.Path).ToArray();
            Assert.Null(original.PMenuGUID);
            Assert.False(await db.Menu.AnyAsync(m=>paths.Contains(m.routerLink),ct));
            var before=(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            var originalGrants=(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)).ToHashSet();
            var users=await db.User.CountAsync(ct);
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodBank"]="true",["HemoSeed:IncludeBankReadPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);db.ChangeTracker.Clear();
            var root=await db.Menu.SingleAsync(m=>m.routerLink=="/Hemo",ct);
            var children=await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct);
            Assert.Equal(3,children.Count);
            Assert.Equal(originalId,Assert.Single(children.Where(m=>m.routerLink=="/hemo/bank")).GUID);
            Assert.All(children,m=>{Assert.False(m.IsPublic);Assert.True(m.ShowInMenu);Assert.Equal(root.Path+m.Gkey+"/",m.Path);});
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.MenuGUID==root.GUID,ct));
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            Assert.True(originalGrants.SetEquals(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)));
            var roles=await db.Role.Where(r=>r.ConvertID!=null).ToListAsync(ct);
            foreach(var route in HemoBankReadRouteSeed.Routes)
            {
                var leaf=Assert.Single(children.Where(m=>m.routerLink==route.Path));
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var expected=new HashSet<(Guid,Guid?)>();
                foreach(var account in accounts.Where(a=>HemoBankReadRouteSeed.CanOpen(route,a.Permissions)))
                {
                    var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                    foreach(var key in route.Mapped.Where(k=>account.Permissions.Contains(k)))
                    {
                        var endpoint=HemoDemoSeed.BankEndpoints.Single(e=>e.Key==key);
                        var api=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                        expected.Add((roleId,api.GUID));
                    }
                }
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
            }
            var count=await db.RoleAccess.CountAsync(ct);var mappings=await db.MenuApi.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(count,await db.RoleAccess.CountAsync(ct));Assert.Equal(mappings,await db.MenuApi.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
            var unitCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodBank"]="true",["HemoSeed:IncludeBankUnitPages"]="true"
            }).Build();
            var unitPaths=HemoBankReadRouteSeed.UnitRoutes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>unitPaths.Contains(m.routerLink),ct));
            await HemoDemoSeed.ApplyAsync(db,unitCfg,ct);db.ChangeTracker.Clear();
            var unitMenus=await db.Menu.Where(m=>unitPaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(2,unitMenus.Count);
            Assert.All(unitMenus,m=>{Assert.False(m.IsPublic);Assert.Equal(root.GUID,m.PMenuGUID);});
            Assert.False(Assert.Single(unitMenus.Where(m=>m.routerLink=="/hemo/bank/unit/:guid")).ShowInMenu);
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            foreach(var route in HemoBankReadRouteSeed.UnitRoutes)
            {
                var leaf=Assert.Single(unitMenus.Where(m=>m.routerLink==route.Path));
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var expected=new HashSet<(Guid,Guid?)>();
                foreach(var account in accounts.Where(a=>HemoBankReadRouteSeed.CanOpen(route,a.Permissions)))
                {
                    var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                    foreach(var key in route.Mapped.Where(k=>account.Permissions.Contains(k)))
                    {
                        var endpoint=HemoDemoSeed.BankEndpoints.Concat(HemoDemoSeed.BloodRequestEndpoints).Single(e=>e.Key==key);
                        var api=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                        expected.Add((roleId,api.GUID));
                    }
                }
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
            }
            var unitCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,unitCfg,ct);await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(unitCount,await db.RoleAccess.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
            var completeCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeBloodBank"]="true",["HemoSeed:IncludeBankCompletePages"]="true"
            }).Build();
            var completePaths=HemoBankReadRouteSeed.CompleteRoutes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>completePaths.Contains(m.routerLink),ct));
            await HemoDemoSeed.ApplyAsync(db,completeCfg,ct);db.ChangeTracker.Clear();
            var completeMenus=await db.Menu.Where(m=>completePaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(4,completeMenus.Count);
            Assert.All(completeMenus,m=>{Assert.False(m.IsPublic);Assert.True(m.ShowInMenu);Assert.Equal(root.GUID,m.PMenuGUID);});
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            foreach(var route in HemoBankReadRouteSeed.CompleteRoutes)
            {
                var leaf=Assert.Single(completeMenus.Where(m=>m.routerLink==route.Path));
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var expected=new HashSet<(Guid,Guid?)>();
                foreach(var account in accounts.Where(a=>HemoBankReadRouteSeed.CanOpen(route,a.Permissions)))
                {
                    var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                    foreach(var key in route.Mapped.Where(k=>account.Permissions.Contains(k)))
                    {
                        var endpoint=HemoDemoSeed.BankEndpoints.Concat(HemoDemoSeed.BloodRequestEndpoints).Single(e=>e.Key==key);
                        var api=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                        expected.Add((roleId,api.GUID));
                    }
                }
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
            }
            var completeCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,completeCfg,ct);await HemoDemoSeed.ApplyAsync(db,unitCfg,ct);
            Assert.Equal(completeCount,await db.RoleAccess.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    public void BankUnitRoutesRequireDetailAndPreserveSupervisorSeparation()
    {
        var receive=HemoBankReadRouteSeed.UnitRoutes.Single(r=>r.Path=="/hemo/bank/receive");
        var unit=HemoBankReadRouteSeed.UnitRoutes.Single(r=>r.Path=="/hemo/bank/unit/:guid");
        Assert.False(HemoBankReadRouteSeed.CanOpen(receive,new[]{"bank-receive"}));
        Assert.True(HemoBankReadRouteSeed.CanOpen(receive,HemoDemoSeed.BankOperatorKeys));
        Assert.True(HemoBankReadRouteSeed.CanOpen(unit,HemoDemoSeed.BankOperatorKeys));
        Assert.True(HemoBankReadRouteSeed.CanOpen(unit,HemoDemoSeed.BankSupervisorKeys));
        Assert.False(HemoBankReadRouteSeed.CanOpen(unit,HemoDemoSeed.DashboardManager.Permissions));
        Assert.DoesNotContain("bank-inspect",HemoDemoSeed.BankOperatorKeys);
        Assert.DoesNotContain("bank-discard",HemoDemoSeed.BankOperatorKeys);
        Assert.Contains("bank-inspect",unit.Mapped);Assert.Contains("bank-discard",unit.Mapped);
        Assert.DoesNotContain("tx-dispatch",unit.Mapped);
    }

    [Fact]
    public void BankCompleteRoutesRequireModeSpecificCapabilities()
    {
        var routes=HemoBankReadRouteSeed.CompleteRoutes;
        var lab=routes.Single(r=>r.Path=="/hemo/bank/crossmatch");
        var thaw=routes.Single(r=>r.Path=="/hemo/bank/thaw");
        var temp=routes.Single(r=>r.Path=="/hemo/bank/temperature");
        Assert.False(HemoBankReadRouteSeed.CanOpen(lab,new[]{"bank-inventory","bank-test","request-bank"}));
        Assert.True(HemoBankReadRouteSeed.CanOpen(lab,new[]{"bank-inventory","bank-test","request-bank","request-bank-detail"}));
        Assert.False(HemoBankReadRouteSeed.CanOpen(thaw,new[]{"bank-inventory"}));
        Assert.True(HemoBankReadRouteSeed.CanOpen(thaw,new[]{"bank-inventory","bank-thaw"}));
        Assert.True(HemoBankReadRouteSeed.CanOpen(temp,new[]{"bank-temperature-history"}));
        Assert.True(HemoBankReadRouteSeed.CanOpen(temp,new[]{"bank-temperature"}));
        Assert.False(HemoBankReadRouteSeed.CanOpen(temp,new[]{"bank-stats"}));
        Assert.DoesNotContain("bank-return",HemoDemoSeed.BankOperatorKeys);
        Assert.Contains("bank-return",HemoDemoSeed.BankSupervisorKeys);
        Assert.Empty(routes.Where(r=>HemoBankReadRouteSeed.CanOpen(r,HemoDemoSeed.DashboardManager.Permissions)));
    }

    [Fact]
    public void TransferReadRoutesRequireModePermissionsWithoutMappingWrites()
    {
        var routes=HemoTransferReadRouteSeed.Routes;
        Assert.Equal(5,routes.Length);
        var accounts=HemoDemoSeed.DemoAccounts(true,true,true);
        var nurse=accounts.Single(a=>a.Key=="Nurse").Permissions;
        var doctor=accounts.Single(a=>a.Key=="Doctor").Permissions;
        var witness=accounts.Single(a=>a.Key=="WitnessNurse").Permissions;
        Assert.Equal(4,routes.Count(r=>HemoTransferReadRouteSeed.CanOpen(r,nurse)));
        Assert.Equal(3,routes.Count(r=>HemoTransferReadRouteSeed.CanOpen(r,doctor)));
        Assert.Equal(2,routes.Count(r=>HemoTransferReadRouteSeed.CanOpen(r,witness)));
        Assert.Empty(routes.Where(r=>HemoTransferReadRouteSeed.CanOpen(r,HemoDemoSeed.DashboardManager.Permissions)));
        Assert.All(routes,r=>Assert.Equal("tx-list",Assert.Single(r.Mapped)));
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task TransferReadMenusPreserveExistingRoleApiPairs()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_TxRead_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,true,true);
            var settings=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            settings["HemoSeed:IncludeTransfusion"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(settings).Build(),ct);
            var original=await db.Menu.SingleAsync(m=>m.routerLink=="/hemo/transfusion",ct);var originalId=original.GUID;
            var paths=HemoTransferReadRouteSeed.Routes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>paths.Contains(m.routerLink),ct));
            var before=(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            var originalGrants=(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)).ToHashSet();
            var users=await db.User.CountAsync(ct);
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeTransfusion"]="true",["HemoSeed:IncludeTransferReadPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);db.ChangeTracker.Clear();
            var root=await db.Menu.SingleAsync(m=>m.routerLink=="/Hemo",ct);
            var children=await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct);
            Assert.Equal(6,children.Count);
            Assert.Equal(originalId,Assert.Single(children.Where(m=>m.routerLink=="/hemo/transfusion")).GUID);
            Assert.All(children,m=>{Assert.False(m.IsPublic);Assert.True(m.ShowInMenu);Assert.Equal(root.Path+m.Gkey+"/",m.Path);});
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.MenuGUID==root.GUID,ct));
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            Assert.True(originalGrants.SetEquals(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)));
            var roles=await db.Role.Where(r=>r.ConvertID!=null).ToListAsync(ct);
            var api=await db.Api.SingleAsync(a=>a.APITypeName=="post"&&a.DownstreamPathTemplate=="/Identity/Transfusion/List",ct);
            foreach(var route in HemoTransferReadRouteSeed.Routes)
            {
                var leaf=Assert.Single(children.Where(m=>m.routerLink==route.Path));
                var expected=accounts.Where(a=>HemoTransferReadRouteSeed.CanOpen(route,a.Permissions))
                    .Select(a=>(roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+a.Key).GUID,(Guid?)api.GUID)).ToHashSet();
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
                Assert.Equal(api.GUID,Assert.Single(await db.MenuApi.Where(m=>m.MenuGUID==leaf.GUID).ToListAsync(ct)).ApiGUID);
            }
            var count=await db.RoleAccess.CountAsync(ct);var mappings=await db.MenuApi.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(count,await db.RoleAccess.CountAsync(ct));Assert.Equal(mappings,await db.MenuApi.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
            var operationCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeTransfusion"]="true",["HemoSeed:IncludeTransferOperationPages"]="true"
            }).Build();
            var operationPaths=HemoTransferReadRouteSeed.OperationRoutes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>operationPaths.Contains(m.routerLink),ct));
            await HemoDemoSeed.ApplyAsync(db,operationCfg,ct);db.ChangeTracker.Clear();
            var operationMenus=await db.Menu.Where(m=>operationPaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(2,operationMenus.Count);
            Assert.All(operationMenus,m=>{Assert.False(m.IsPublic);Assert.Equal(root.GUID,m.PMenuGUID);});
            Assert.False(Assert.Single(operationMenus.Where(m=>m.routerLink=="/hemo/transfusion/detail/:guid")).ShowInMenu);
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            foreach(var route in HemoTransferReadRouteSeed.OperationRoutes)
            {
                var leaf=Assert.Single(operationMenus.Where(m=>m.routerLink==route.Path));
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var expected=new HashSet<(Guid,Guid?)>();
                foreach(var account in accounts.Where(a=>HemoTransferReadRouteSeed.CanOpen(route,a.Permissions)))
                {
                    var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                    foreach(var key in route.Mapped.Where(k=>account.Permissions.Contains(k)))
                    {
                        var endpoint=HemoDemoSeed.TransferEndpoints.Single(e=>e.Key==key);
                        var mappedApi=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                        expected.Add((roleId,mappedApi.GUID));
                    }
                }
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
            }
            var operationCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,operationCfg,ct);await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(operationCount,await db.RoleAccess.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    public void TransferOperationsPreserveRoleSeparation()
    {
        var dispatch=HemoTransferReadRouteSeed.OperationRoutes.Single(r=>r.Path=="/hemo/transfusion/dispatch");
        var detail=HemoTransferReadRouteSeed.OperationRoutes.Single(r=>r.Path=="/hemo/transfusion/detail/:guid");
        Assert.False(HemoTransferReadRouteSeed.CanOpen(dispatch,new[]{"tx-candidates","tx-dispatch"}));
        Assert.True(HemoTransferReadRouteSeed.CanOpen(dispatch,HemoDemoSeed.TransferSupervisorKeys));
        Assert.False(HemoTransferReadRouteSeed.CanOpen(dispatch,HemoDemoSeed.TransferNurseKeys));
        Assert.True(HemoTransferReadRouteSeed.CanOpen(detail,HemoDemoSeed.WitnessNurse.Permissions));
        Assert.DoesNotContain("tx-start",HemoDemoSeed.WitnessNurse.Permissions);
        Assert.DoesNotContain("tx-dispatch",detail.Mapped);
        Assert.False(HemoTransferReadRouteSeed.CanOpen(detail,HemoDemoSeed.DashboardManager.Permissions));
    }

    [Fact]
    public void HvReadQueuesPreserveRoleEligibility()
    {
        var routes=HemoHvReadRouteSeed.Routes;
        Assert.Equal(5,routes.Length);
        var accounts=HemoDemoSeed.DemoAccounts(true,true,true,true);
        var nurse=accounts.Single(a=>a.Key=="Nurse").Permissions;
        var doctor=accounts.Single(a=>a.Key=="Doctor").Permissions;
        var bank=accounts.Single(a=>a.Key=="BloodBank").Permissions;
        Assert.Equal(2,routes.Count(r=>HemoHvReadRouteSeed.CanOpen(r,nurse)));
        Assert.Equal(3,routes.Count(r=>HemoHvReadRouteSeed.CanOpen(r,doctor)));
        Assert.Equal(3,routes.Count(r=>HemoHvReadRouteSeed.CanOpen(r,bank)));
        Assert.Equal(3,routes.Count(r=>HemoHvReadRouteSeed.CanOpen(r,HemoDemoSeed.HvOfficer.Permissions)));
        Assert.Equal(2,routes.Count(r=>HemoHvReadRouteSeed.CanOpen(r,HemoDemoSeed.HvReviewer.Permissions)));
        Assert.Empty(routes.Where(r=>HemoHvReadRouteSeed.CanOpen(r,HemoDemoSeed.DashboardManager.Permissions)));
        Assert.All(routes,r=>Assert.Equal("hv-list",Assert.Single(r.Mapped)));
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task HvReadMenusPreserveExistingRoleApiPairs()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_HvRead_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,true,true,true);
            var settings=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            settings["HemoSeed:IncludeHemovigilance"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(settings).Build(),ct);
            var original=await db.Menu.SingleAsync(m=>m.routerLink=="/hemo/hv",ct);var originalId=original.GUID;
            var paths=HemoHvReadRouteSeed.Routes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>paths.Contains(m.routerLink),ct));
            var before=(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            var originalGrants=(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)).ToHashSet();
            var users=await db.User.CountAsync(ct);
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeHemovigilance"]="true",["HemoSeed:IncludeHvReadPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);db.ChangeTracker.Clear();
            var root=await db.Menu.SingleAsync(m=>m.routerLink=="/Hemo",ct);
            var children=await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct);
            Assert.Equal(6,children.Count);
            Assert.Equal(originalId,Assert.Single(children.Where(m=>m.routerLink=="/hemo/hv")).GUID);
            Assert.All(children,m=>{Assert.False(m.IsPublic);Assert.True(m.ShowInMenu);Assert.Equal(root.Path+m.Gkey+"/",m.Path);});
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.MenuGUID==root.GUID,ct));
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            Assert.True(originalGrants.SetEquals(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)));
            var roles=await db.Role.Where(r=>r.ConvertID!=null).ToListAsync(ct);
            var api=await db.Api.SingleAsync(a=>a.APITypeName=="post"&&a.DownstreamPathTemplate=="/Identity/Hemovigilance/List",ct);
            foreach(var route in HemoHvReadRouteSeed.Routes)
            {
                var leaf=Assert.Single(children.Where(m=>m.routerLink==route.Path));
                var expected=accounts.Where(a=>HemoHvReadRouteSeed.CanOpen(route,a.Permissions))
                    .Select(a=>(roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+a.Key).GUID,(Guid?)api.GUID)).ToHashSet();
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
                Assert.Equal(api.GUID,Assert.Single(await db.MenuApi.Where(m=>m.MenuGUID==leaf.GUID).ToListAsync(ct)).ApiGUID);
            }
            var count=await db.RoleAccess.CountAsync(ct);var mappings=await db.MenuApi.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(count,await db.RoleAccess.CountAsync(ct));Assert.Equal(mappings,await db.MenuApi.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
            var opCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeHemovigilance"]="true",["HemoSeed:IncludeHvOperationPages"]="true"
            }).Build();
            var opPaths=HemoHvReadRouteSeed.OperationRoutes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>opPaths.Contains(m.routerLink),ct));
            await HemoDemoSeed.ApplyAsync(db,opCfg,ct);db.ChangeTracker.Clear();
            var opMenus=await db.Menu.Where(m=>opPaths.Contains(m.routerLink)).ToListAsync(ct);
            Assert.Equal(2,opMenus.Count);
            Assert.All(opMenus,m=>{Assert.False(m.IsPublic);Assert.Equal(root.GUID,m.PMenuGUID);});
            Assert.False(Assert.Single(opMenus.Where(m=>m.routerLink=="/hemo/hv/detail/:guid")).ShowInMenu);
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            foreach(var route in HemoHvReadRouteSeed.OperationRoutes)
            {
                var leaf=Assert.Single(opMenus.Where(m=>m.routerLink==route.Path));
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                var expected=new HashSet<(Guid,Guid?)>();
                foreach(var account in accounts.Where(a=>HemoHvReadRouteSeed.CanOpen(route,a.Permissions)))
                {
                    var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                    foreach(var key in route.Mapped.Where(k=>account.Permissions.Contains(k)))
                    {
                        var endpoint=HemoDemoSeed.HvEndpoints.Single(e=>e.Key==key);
                        var mappedApi=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                        expected.Add((roleId,mappedApi.GUID));
                    }
                }
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
            }
            var opCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,opCfg,ct);
            Assert.Equal(opCount,await db.RoleAccess.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
            var reportCfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeHemovigilance"]="true",["HemoSeed:IncludeHvReportPages"]="true"
            }).Build();
            var reportRoute=Assert.Single(HemoHvReadRouteSeed.ReportRoutes);
            Assert.False(await db.Menu.AnyAsync(m=>m.routerLink==reportRoute.Path,ct));
            await HemoDemoSeed.ApplyAsync(db,reportCfg,ct);db.ChangeTracker.Clear();
            var reportMenu=await db.Menu.SingleAsync(m=>m.routerLink==reportRoute.Path,ct);
            Assert.False(reportMenu.IsPublic);Assert.True(reportMenu.ShowInMenu);Assert.Equal(root.GUID,reportMenu.PMenuGUID);
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            var expectedReports=new HashSet<(Guid,Guid?)>();
            foreach(var account in accounts.Where(a=>HemoHvReadRouteSeed.CanOpen(reportRoute,a.Permissions)))
            {
                var roleId=roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key).GUID;
                foreach(var key in reportRoute.Mapped.Where(k=>account.Permissions.Contains(k)))
                {
                    var endpoint=HemoDemoSeed.HvEndpoints.Single(e=>e.Key==key);
                    var mappedApi=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                    expectedReports.Add((roleId,mappedApi.GUID));
                }
            }
            var reportGrants=await db.RoleAccess.Where(r=>r.MenuGUID==reportMenu.GUID).ToListAsync(ct);
            Assert.True(expectedReports.SetEquals(reportGrants.Select(g=>(g.RoleGUID,g.ApiGUID))));
            var reportCount=await db.RoleAccess.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,reportCfg,ct);
            Assert.Equal(reportCount,await db.RoleAccess.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }

    [Fact]
    public void HvOperationRoutesPreserveIndependentReview()
    {
        var report=HemoHvReadRouteSeed.OperationRoutes.Single(r=>r.Path=="/hemo/hv/new");
        var detail=HemoHvReadRouteSeed.OperationRoutes.Single(r=>r.Path=="/hemo/hv/detail/:guid");
        Assert.False(HemoHvReadRouteSeed.CanOpen(report,new[]{"hv-candidates","hv-report"}));
        Assert.True(HemoHvReadRouteSeed.CanOpen(report,HemoDemoSeed.HvOfficer.Permissions));
        Assert.False(HemoHvReadRouteSeed.CanOpen(report,HemoDemoSeed.HvReviewer.Permissions));
        Assert.True(HemoHvReadRouteSeed.CanOpen(detail,HemoDemoSeed.HvReviewer.Permissions));
        Assert.DoesNotContain("hv-verify",HemoDemoSeed.HvOfficer.Permissions);
        Assert.DoesNotContain("hv-assign",HemoDemoSeed.HvReviewer.Permissions);
        Assert.DoesNotContain("hv-report",detail.Mapped);
        Assert.False(HemoHvReadRouteSeed.CanOpen(detail,HemoDemoSeed.DashboardManager.Permissions));
    }

    [Fact]
    public void HvReportRouteRequiresListAndDetailAndPreservesWriteSeparation()
    {
        var route=Assert.Single(HemoHvReadRouteSeed.ReportRoutes);
        Assert.False(HemoHvReadRouteSeed.CanOpen(route,new[]{"hv-reports"}));
        Assert.False(HemoHvReadRouteSeed.CanOpen(route,new[]{"hv-report-detail"}));
        Assert.True(HemoHvReadRouteSeed.CanOpen(route,HemoDemoSeed.HvOfficer.Permissions));
        Assert.True(HemoHvReadRouteSeed.CanOpen(route,HemoDemoSeed.HvReviewer.Permissions));
        Assert.False(HemoHvReadRouteSeed.CanOpen(route,HemoDemoSeed.DashboardManager.Permissions));
        Assert.DoesNotContain("hv-archive",HemoDemoSeed.HvReviewer.Permissions);
        Assert.DoesNotContain("hv-report-external",HemoDemoSeed.HvReviewer.Permissions);
        Assert.Contains("hv-archive",route.Mapped);
    }

    [Fact]
    [Trait("Category","SqlServer")]
    public async Task QualityReadMenusPreserveExistingRoleApiPairs()
    {
        var connection=Environment.GetEnvironmentVariable("HEMO_TEST_SQL");
        Assert.False(string.IsNullOrWhiteSpace(connection));
        var cs=new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection){InitialCatalog="Hemo_QualityRead_"+Guid.NewGuid().ToString("N")};
        var auth=new AuthService(new HttpContextAccessor());
        var options=new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(cs.ConnectionString)
            .AddInterceptors(new Audit(),new DispatchDomainEventsInterceptor(null!)).Options;
        await using var db=new ApplicationDbContext(options,auth);var ct=CancellationToken.None;
        try
        {
            await db.Database.MigrateAsync(ct);
            var accounts=HemoDemoSeed.DemoAccounts(true,true,true,true,true);
            var settings=accounts.ToDictionary(a=>"HemoSeed:Passwords:"+a.Key,a=>(string?)("T!"+Guid.NewGuid().ToString("N")));
            settings["HemoSeed:IncludeQuality"]="true";
            await HemoDemoSeed.ApplyAsync(db,new ConfigurationBuilder().AddInMemoryCollection(settings).Build(),ct);
            var original=await db.Menu.SingleAsync(m=>m.routerLink=="/hemo/quality",ct);var originalId=original.GUID;
            var paths=HemoQualityReadRouteSeed.Routes.Select(r=>r.Path).ToArray();
            Assert.False(await db.Menu.AnyAsync(m=>paths.Contains(m.routerLink),ct));
            var before=(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)).ToHashSet();
            var originalGrants=(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)).ToHashSet();
            var users=await db.User.CountAsync(ct);
            var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
                ["HemoSeed:IncludeQuality"]="true",["HemoSeed:IncludeQualityReadPages"]="true"
            }).Build();
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);db.ChangeTracker.Clear();
            var root=await db.Menu.SingleAsync(m=>m.routerLink=="/Hemo",ct);
            var children=await db.Menu.Where(m=>m.PMenuGUID==root.GUID).ToListAsync(ct);
            Assert.Equal(10,children.Count);
            Assert.Equal(originalId,Assert.Single(children.Where(m=>m.routerLink=="/hemo/quality")).GUID);
            Assert.All(children,m=>{Assert.False(m.IsPublic);Assert.True(m.ShowInMenu);Assert.Equal(root.Path+m.Gkey+"/",m.Path);});
            Assert.False(await db.RoleAccess.AnyAsync(r=>r.MenuGUID==root.GUID,ct));
            Assert.True(before.SetEquals(await db.RoleAccess.Select(r=>new{r.RoleGUID,r.ApiGUID}).Distinct().ToListAsync(ct)));
            Assert.True(originalGrants.SetEquals(await db.RoleAccess.Where(r=>r.MenuGUID==originalId).Select(r=>new{r.RoleGUID,r.ApiGUID}).ToListAsync(ct)));
            var roles=await db.Role.Where(r=>r.ConvertID!=null).ToListAsync(ct);
            foreach(var route in HemoQualityReadRouteSeed.Routes)
            {
                var leaf=Assert.Single(children.Where(m=>m.routerLink==route.Path));
                var endpoint=HemoDemoSeed.QualityEndpoints.Single(e=>e.Key==Assert.Single(route.Mapped));
                var api=await db.Api.SingleAsync(a=>a.APITypeName==endpoint.Method&&a.DownstreamPathTemplate==endpoint.Path,ct);
                var expected=accounts.Where(a=>HemoQualityReadRouteSeed.CanOpen(route,a.Permissions))
                    .Select(a=>(roles.Single(r=>r.ConvertID=="HEMO-DEMO-ST02:"+a.Key).GUID,(Guid?)api.GUID)).ToHashSet();
                var actual=await db.RoleAccess.Where(r=>r.MenuGUID==leaf.GUID).ToListAsync(ct);
                Assert.True(expected.SetEquals(actual.Select(g=>(g.RoleGUID,g.ApiGUID))));
                Assert.Equal(api.GUID,Assert.Single(await db.MenuApi.Where(m=>m.MenuGUID==leaf.GUID).ToListAsync(ct)).ApiGUID);
            }
            var count=await db.RoleAccess.CountAsync(ct);var mappings=await db.MenuApi.CountAsync(ct);
            await HemoDemoSeed.ApplyAsync(db,cfg,ct);
            Assert.Equal(count,await db.RoleAccess.CountAsync(ct));Assert.Equal(mappings,await db.MenuApi.CountAsync(ct));Assert.Equal(users,await db.User.CountAsync(ct));
        }
        finally{await db.Database.EnsureDeletedAsync(ct);}
    }
    [Fact]
    public void QualityReadRoutesPreserveRoleSeparation()
    {
        var routes=HemoQualityReadRouteSeed.Routes;
        Assert.Equal(9,routes.Length);
        var tech=HemoDemoSeed.QualityAccounts.Single(a=>a.Key=="EquipmentTech").Permissions;
        var reviewer=HemoDemoSeed.QualityAccounts.Single(a=>a.Key=="EquipmentReviewer").Permissions;
        var officer=HemoDemoSeed.QualityAccounts.Single(a=>a.Key=="QualityOfficer").Permissions;
        Assert.Equal(5,routes.Count(r=>HemoQualityReadRouteSeed.CanOpen(r,tech)));
        Assert.Equal(3,routes.Count(r=>HemoQualityReadRouteSeed.CanOpen(r,reviewer)));
        Assert.Equal(4,routes.Count(r=>HemoQualityReadRouteSeed.CanOpen(r,officer)));
        Assert.Single(routes.Where(r=>HemoQualityReadRouteSeed.CanOpen(r,HemoDemoSeed.DashboardManager.Permissions)));
        Assert.All(routes,r=>Assert.Single(r.Mapped));
    }
}
