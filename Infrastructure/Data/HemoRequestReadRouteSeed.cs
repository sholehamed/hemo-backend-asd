using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

// Opt-in route aliases for the R02 read-only request pages. Reuse the
// existing endpoint and role grants; never grant a mutation on a queue.
public static class HemoRequestReadRouteSeed
{
    public sealed record Route(string Path,string Label,string Icon,bool Visible,string[] Required,string[] Mapped);
    public static readonly Route[] Routes=[
        new("/hemo/requests/bank","صف درخواست بانک خون","pi pi-inbox",true,["request-bank"],["request-bank"]),
        new("/hemo/requests/approval","کارتابل تأیید پزشک","pi pi-check-circle",true,["request-list","request-decide"],["request-list"]),
        new("/hemo/requests/doctor","کارتابل پزشک","pi pi-user",false,["request-list","request-decide"],["request-list"])
    ];
    public static readonly Route[] DetailRoutes=[
        new("/hemo/requests/detail/:guid","جزئیات درخواست خون","pi pi-file",false,["request-detail"],["request-detail","request-submit","request-cancel","request-decide"]),
        new("/hemo/requests/bank/:guid","جزئیات صف بانک خون","pi pi-inbox",false,["request-bank-detail"],["request-bank-detail"])
    ];
    public static bool CanOpen(Route route,IEnumerable<string> keys)
    {
        var allowed=keys.ToHashSet(StringComparer.Ordinal);
        return route.Required.All(allowed.Contains);
    }

    internal static async Task ApplyAsync(ApplicationDbContext db,Menu original,
        IReadOnlyDictionary<string,Guid> apis,IEnumerable<HemoDemoSeed.Account> accounts,CancellationToken ct,bool includeDetailPages=false)
    {
        const string rootMarker="HEMO-R02-COMPANY-MENU";
        const string prefix="HEMO-R02-REQUEST:";
        var root=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/Hemo",ct);
        if(root==null)
        {
            root=new Menu{routerLink="/Hemo",label="هموویژیلانس",icon="pi pi-heart",ShowInMenu=true,IsPublic=false,ConvertID=rootMarker};
            db.Menu.Add(root);await db.SaveChangesAsync(ct);
        }
        else if(root.ConvertID!=rootMarker||root.PMenuGUID!=null||root.IsPublic||!root.ShowInMenu)
            throw new InvalidOperationException("Existing /Hemo menu is not owned by the demo pilot.");
        // Preserve the original menu GUID and all grants. Adopt only the exact
        // legacy demo leaf shape; reject an externally configured hierarchy.
        if(original.IsPublic||!original.ShowInMenu)
            throw new InvalidOperationException("Existing request menu is not a private visible demo menu.");
        if(original.PMenuGUID!=root.GUID)
        {
            var requestApiIds=HemoDemoSeed.BloodRequestEndpoints.Select(e=>apis[e.Key]).ToArray();
            if(original.PMenuGUID!=null||original.ConvertID!=null||
                original.label!="درخواست خون"||original.icon!="pi pi-file-edit"||original.Gorder!=2||
                await db.Menu.AnyAsync(m=>m.PMenuGUID==original.GUID,ct)||
                await db.MenuApi.AnyAsync(m=>m.MenuGUID==original.GUID&&!requestApiIds.Contains(m.ApiGUID),ct))
                throw new InvalidOperationException("Existing request menu conflicts with R02 demo promotion.");
            original.PMenuGUID=root.GUID;
            await db.SaveChangesAsync(ct);
            if(original.Path!=root.Path+original.Gkey+"/")
                throw new InvalidOperationException("Request menu path was not recalculated.");
        }
        var roles=new List<(Guid Id,HashSet<string> Keys)>();
        foreach(var account in accounts)
        {
            var role=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key,ct);
            var grants=(await db.RoleAccess.Where(r=>r.RoleGUID==role.GUID&&r.MenuGUID==original.GUID&&r.ApiGUID.HasValue)
                .Select(r=>r.ApiGUID!.Value).ToListAsync(ct)).ToHashSet();
            roles.Add((role.GUID,account.Permissions.Where(key=>apis.TryGetValue(key,out var id)&&grants.Contains(id)).ToHashSet(StringComparer.Ordinal)));
        }
        foreach(var route in includeDetailPages?Routes.Concat(DetailRoutes):Routes)
        {
            var marker=prefix+route.Path;
            var menu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink==route.Path,ct);
            if(menu==null)
            {
                menu=new Menu{routerLink=route.Path,label=route.Label,icon=route.Icon,ShowInMenu=route.Visible,
                    IsPublic=false,PMenuGUID=root.GUID,ConvertID=marker};
                db.Menu.Add(menu);await db.SaveChangesAsync(ct);
            }
            else if(menu.ConvertID!=marker||menu.IsPublic||menu.ShowInMenu!=route.Visible||menu.PMenuGUID!=root.GUID)
                throw new InvalidOperationException("Existing menu conflicts with R02 request seed: "+route.Path);
            foreach(var key in route.Mapped)
            {
                var id=apis[key];
                if(!await db.MenuApi.AnyAsync(m=>m.MenuGUID==menu.GUID&&m.ApiGUID==id,ct))
                {db.MenuApi.Add(new MenuApi{MenuGUID=menu.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
                foreach(var role in roles.Where(r=>CanOpen(route,r.Keys)&&r.Keys.Contains(key)))
                    if(!await db.RoleAccess.AnyAsync(r=>r.RoleGUID==role.Id&&r.MenuGUID==menu.GUID&&r.ApiGUID==id,ct))
                    {db.RoleAccess.Add(new RoleAccess{RoleGUID=role.Id,MenuGUID=menu.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
            }
        }
    }
}
