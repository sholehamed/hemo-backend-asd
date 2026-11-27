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
    public static bool CanOpen(Route route,IEnumerable<string> permissionKeys)
    {
        var allowed=permissionKeys.ToHashSet(StringComparer.Ordinal);
        return route.Required.All(allowed.Contains);
    }

    internal static async Task ApplyAsync(ApplicationDbContext db,Guid admissionMenuGuid,
        IReadOnlyDictionary<string,Guid> apis,IEnumerable<HemoDemoSeed.Account> accounts,CancellationToken ct)
    {
        const string prefix="HEMO-R02-ADMISSION:";
        var existingAdmission=await db.Menu.SingleAsync(m=>m.GUID==admissionMenuGuid,ct);
        // Keep existing parent menu navigation intact: visible pilot pages are siblings.
        // Package hierarchy/subpath integration is a separate approval gate.
        var roles=new List<(Guid Id,HashSet<string> Keys)>();
        foreach(var account in accounts)
        {
            var role=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key,ct);
            var grants=(await db.RoleAccess.Where(r=>r.RoleGUID==role.GUID&&r.MenuGUID==admissionMenuGuid&&r.ApiGUID.HasValue)
                .Select(r=>r.ApiGUID!.Value).ToListAsync(ct)).ToHashSet();
            roles.Add((role.GUID,account.Permissions.Where(key=>apis.TryGetValue(key,out var id)&&grants.Contains(id)).ToHashSet(StringComparer.Ordinal)));
        }
        foreach(var route in Routes)
        {
            var marker=prefix+route.Path;
            var menu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink==route.Path,ct);
            if(menu==null)
            {
                menu=new Menu{routerLink=route.Path,label=route.Label,icon=route.Icon,ShowInMenu=route.ShowInMenu,
                    IsPublic=false,PMenuGUID=existingAdmission.PMenuGUID,ConvertID=marker};
                db.Menu.Add(menu);await db.SaveChangesAsync(ct);
            }
            else if(menu.ConvertID!=marker||menu.IsPublic||menu.ShowInMenu!=route.ShowInMenu||menu.PMenuGUID!=existingAdmission.PMenuGUID)
                throw new InvalidOperationException("Existing menu conflicts with R02 admission seed: "+route.Path);
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
