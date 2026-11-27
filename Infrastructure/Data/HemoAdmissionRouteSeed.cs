using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

// Opt-in demo-only pilot for the exact page patterns used by the company AuthGuard.
// No new API, account, role, public permission or whole-menu grants are created.
public static class HemoAdmissionRouteSeed
{
    public sealed record Route(string Path,string Label,string Icon,bool ShowInMenu,string[] Required,string[] Optional);
    public static readonly Route[] Routes=[
        new("/admission/new","پذیرش بیمار جدید","pi pi-plus",true,["create","find","detail"],[]),
        new("/admission/list","فهرست بیماران بستری","pi pi-list",true,["list"],["stats"]),
        new("/admission/detail/:guid","جزئیات پذیرش","pi pi-id-card",false,["detail"],["update","identity"]),
        new("/admission/identity","تطبیق هویت","pi pi-check-circle",true,["list","detail","reviews"],["review"]),
        new("/admission/history","بررسی سوابق انتقال خون","pi pi-history",true,["list","detail","reviews","history"],["review"]),
        new("/admission/discharge","کنترل ترخیص","pi pi-sign-out",true,["list","detail","reviews"],["review"]),
        new("/admission/his","همگام‌سازی HIS","pi pi-sync",true,["list"],[]),
        new("/admission/stats","گزارش‌های پذیرش","pi pi-chart-bar",true,["stats"],[])
    ];
    public static readonly Route DashboardRoute=new("/admission/dashboard","داشبورد پذیرش","pi pi-chart-pie",true,[],["stats","list","detail","create","find","reviews","history"]);
    public static bool CanOpen(Route route,IEnumerable<string> permissionKeys)
    {
        var allowed=permissionKeys.ToHashSet(StringComparer.Ordinal);
        return route.Path==DashboardRoute.Path?allowed.Contains("stats")||allowed.Contains("list"):route.Required.All(allowed.Contains);
    }

    internal static async Task ApplyAsync(ApplicationDbContext db,Guid admissionMenuGuid,
        IReadOnlyDictionary<string,Guid> apis,IEnumerable<HemoDemoSeed.Account> accounts,CancellationToken ct,bool includeCompanyMenuPilot=false,bool includeReadPages=false,bool includeCompletePages=false)
    {
        const string prefix="HEMO-R02-ADMISSION:";
        var existingAdmission=await db.Menu.SingleAsync(m=>m.GUID==admissionMenuGuid,ct);
        // Only activated pages are exposed under the company SubPath.
        // Read pages require a separate opt-in for deployments on the earlier frontend.
        var companyRoot=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/Hemo",ct);
        const string rootMarker="HEMO-R02-COMPANY-MENU";
        includeReadPages |= includeCompletePages;
        includeCompanyMenuPilot |= includeReadPages;
        if(includeCompanyMenuPilot)
        {
            if(companyRoot==null)
            {
                companyRoot=new Menu{routerLink="/Hemo",label="هموویژیلانس",icon="pi pi-heart",ShowInMenu=true,IsPublic=false,ConvertID=rootMarker};
                db.Menu.Add(companyRoot);await db.SaveChangesAsync(ct);
            }
            else if(companyRoot.ConvertID!=rootMarker||companyRoot.PMenuGUID!=null||companyRoot.IsPublic||!companyRoot.ShowInMenu)
                throw new InvalidOperationException("Existing /Hemo menu is not owned by the demo pilot; configure it through the company menu administration.");
        }
        var ownedRoot=companyRoot?.ConvertID==rootMarker&&companyRoot.PMenuGUID==null&&!companyRoot.IsPublic&&companyRoot.ShowInMenu?companyRoot:null;
        var roles=new List<(Guid Id,HashSet<string> Keys)>();
        foreach(var account in accounts)
        {
            var role=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key,ct);
            var grants=(await db.RoleAccess.Where(r=>r.RoleGUID==role.GUID&&r.MenuGUID==admissionMenuGuid&&r.ApiGUID.HasValue)
                .Select(r=>r.ApiGUID!.Value).ToListAsync(ct)).ToHashSet();
            roles.Add((role.GUID,account.Permissions.Where(key=>apis.TryGetValue(key,out var id)&&grants.Contains(id)).ToHashSet(StringComparer.Ordinal)));
        }
        // The original /admission menu is not seed-marked; never move it.
        // The separate dashboard alias is added only by the complete opt-in.
        var dashboardExists=await db.Menu.AnyAsync(m=>m.routerLink==DashboardRoute.Path&&m.ConvertID==prefix+DashboardRoute.Path,ct);
        var routes=includeCompletePages||dashboardExists?Routes.Append(DashboardRoute):Routes.AsEnumerable();
        foreach(var route in routes)
        {
            var marker=prefix+route.Path;
            var menu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink==route.Path,ct);
            var active=includeCompletePages||route.Path=="/admission/stats"&&includeCompanyMenuPilot||
                includeReadPages&&(route.Path=="/admission/list"||route.Path=="/admission/his");
            var previouslyAttached=ownedRoot!=null&&menu?.PMenuGUID==ownedRoot.GUID;
            var parent=ownedRoot!=null&&(active||previouslyAttached)?ownedRoot.GUID:existingAdmission.PMenuGUID;
            if(menu==null)
            {
                menu=new Menu{routerLink=route.Path,label=route.Label,icon=route.Icon,ShowInMenu=route.ShowInMenu,
                    IsPublic=false,PMenuGUID=parent,ConvertID=marker};
                db.Menu.Add(menu);await db.SaveChangesAsync(ct);
            }
            else if(menu.ConvertID!=marker||menu.IsPublic||menu.ShowInMenu!=route.ShowInMenu )
                throw new InvalidOperationException("Existing menu conflicts with R02 admission seed: "+route.Path);
            else if(menu.PMenuGUID!=parent)
            {
                if(!active||ownedRoot==null||menu.PMenuGUID!=existingAdmission.PMenuGUID||
                    await db.Menu.AnyAsync(m=>m.PMenuGUID==menu.GUID,ct))
                    throw new InvalidOperationException("Existing menu hierarchy conflicts with R02 admission seed: "+route.Path);
                // Reparent only the seed-owned leaf. Menu.OnModified recalculates its path.
                menu.PMenuGUID=parent;
                await db.SaveChangesAsync(ct);
                if(menu.Path!=ownedRoot.Path+menu.Gkey+"/")
                    throw new InvalidOperationException("Company menu path was not recalculated.");
            }
            // For create, detail is required for the successful navigation to the
            // detail page; do not map a detail GET onto the create form itself.
            var keys=route.Required.Concat(route.Optional).Where(k=>route.Path!="/admission/new"||k!="detail").Distinct().ToArray();
            foreach(var key in keys)
            {
                var id=apis[key];
                if(!await db.MenuApi.AnyAsync(m=>m.MenuGUID==menu.GUID&&m.ApiGUID==id,ct))
                {db.MenuApi.Add(new MenuApi{MenuGUID=menu.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
            }
            foreach(var role in roles.Where(r=>CanOpen(route,r.Keys)))
                foreach(var key in keys.Where(role.Keys.Contains))
                {
                    var id=apis[key];
                    if(!await db.RoleAccess.AnyAsync(r=>r.RoleGUID==role.Id&&r.MenuGUID==menu.GUID&&r.ApiGUID==id,ct))
                    {db.RoleAccess.Add(new RoleAccess{RoleGUID=role.Id,MenuGUID=menu.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
                }
        }
    }
}
