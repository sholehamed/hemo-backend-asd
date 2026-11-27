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
}
