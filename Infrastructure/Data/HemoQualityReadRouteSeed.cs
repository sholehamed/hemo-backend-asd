using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class HemoQualityReadRouteSeed
{
    public sealed record Route(string Path,string Label,string Icon,string Required,string[] Mapped,string[]? AnyOf=null,bool Visible=true,string[]? AllOf=null);
    public static readonly Route[] Routes=[
        new("/hemo/quality/equipment-dashboard","داشبورد تجهیزات","pi pi-chart-bar","quality-equipment-stats",["quality-equipment-stats"]),
        new("/hemo/quality/equipment","شناسنامه تجهیزات","pi pi-cog","quality-equipment-list",["quality-equipment-list"]),
        new("/hemo/quality/maintenance","سررسید سرویس و کالیبراسیون","pi pi-calendar","quality-equipment-list",["quality-equipment-list"],null,true,["quality-equipment-service"]),
        new("/hemo/quality/temperature","پایش دمای تجهیزات","pi pi-sun","quality-equipment-list",["quality-equipment-list"],null,true,["quality-equipment-temperature"]),
        new("/hemo/quality/incidents","حوادث تجهیزات","pi pi-exclamation-triangle","quality-incidents",["quality-incidents"]),
        new("/hemo/quality/qc","کنترل کیفیت آزمایشگاه","pi pi-check-square","quality-qc-list",["quality-qc-list"]),
        new("/hemo/quality/committee","کمیته انتقال خون","pi pi-users","quality-committee-list",["quality-committee-list"]),
        new("/hemo/quality/audit","ممیزی و ارزیابی","pi pi-file","quality-audit-list",["quality-audit-list"]),
        new("/hemo/quality/training","آموزش کارکنان","pi pi-book","quality-training-list",["quality-training-list"])
    ];
    public static bool CanOpen(Route route,IEnumerable<string> keys)
    {
        var allowed=keys.ToHashSet(StringComparer.Ordinal);
        return allowed.Contains(route.Required)&&(route.AnyOf==null||route.AnyOf.Any(allowed.Contains))&&(route.AllOf==null||route.AllOf.All(allowed.Contains));
    }

    internal static async Task ApplyAsync(ApplicationDbContext db,Menu original,IReadOnlyDictionary<string,Guid> apis,
        IEnumerable<HemoDemoSeed.Account> accounts,CancellationToken ct)
    {
        const string rootMarker="HEMO-R02-COMPANY-MENU";
        var root=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/Hemo",ct);
        if(root==null)
        {
            root=new Menu{routerLink="/Hemo",label="هموویژیلانس",icon="pi pi-heart",ShowInMenu=true,IsPublic=false,ConvertID=rootMarker};
            db.Menu.Add(root);await db.SaveChangesAsync(ct);
        }
        else if(root.ConvertID!=rootMarker||root.PMenuGUID!=null||root.IsPublic||!root.ShowInMenu)
            throw new InvalidOperationException("Existing /Hemo menu is not owned by demo seed.");
        if(original.IsPublic||!original.ShowInMenu)throw new InvalidOperationException("Existing quality menu is not private and visible.");
        if(original.PMenuGUID!=root.GUID)
        {
            var ids=HemoDemoSeed.QualityEndpoints.Select(e=>apis[e.Key]).ToArray();
            if(original.PMenuGUID!=null||original.ConvertID!=null||original.label!="تجهیزات و مدیریت کیفیت"||original.icon!="pi pi-cog"||original.Gorder!=6||
                await db.Menu.AnyAsync(m=>m.PMenuGUID==original.GUID,ct)||
                await db.MenuApi.AnyAsync(m=>m.MenuGUID==original.GUID&&!ids.Contains(m.ApiGUID),ct))
                throw new InvalidOperationException("Existing quality menu conflicts with R02 demo promotion.");
            original.PMenuGUID=root.GUID;await db.SaveChangesAsync(ct);
            if(original.Path!=root.Path+original.Gkey+"/")throw new InvalidOperationException("Quality menu path was not recalculated.");
        }
        var roles=new List<(Guid Id,HashSet<string> Keys)>();
        foreach(var account in accounts)
        {
            var role=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key,ct);
            var grants=(await db.RoleAccess.Where(r=>r.RoleGUID==role.GUID&&r.MenuGUID==original.GUID&&r.ApiGUID.HasValue)
                .Select(r=>r.ApiGUID!.Value).ToListAsync(ct)).ToHashSet();
            roles.Add((role.GUID,account.Permissions.Where(k=>apis.TryGetValue(k,out var id)&&grants.Contains(id)).ToHashSet(StringComparer.Ordinal)));
        }
        foreach(var route in Routes)
        {
            var marker="HEMO-R02-QUALITY:"+route.Path;
            var leaf=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink==route.Path,ct);
            if(leaf==null)
            {
                leaf=new Menu{routerLink=route.Path,label=route.Label,icon=route.Icon,ShowInMenu=route.Visible,IsPublic=false,PMenuGUID=root.GUID,ConvertID=marker};
                db.Menu.Add(leaf);await db.SaveChangesAsync(ct);
            }
            else if(leaf.ConvertID!=marker||leaf.IsPublic||leaf.ShowInMenu!=route.Visible||leaf.PMenuGUID!=root.GUID)
                throw new InvalidOperationException("Existing quality route conflicts with R02 seed: "+route.Path);
            foreach(var key in route.Mapped)
            {
                var id=apis[key];
                if(!await db.MenuApi.AnyAsync(m=>m.MenuGUID==leaf.GUID&&m.ApiGUID==id,ct))
                {db.MenuApi.Add(new MenuApi{MenuGUID=leaf.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
                foreach(var role in roles.Where(r=>CanOpen(route,r.Keys)&&r.Keys.Contains(key)))
                    if(!await db.RoleAccess.AnyAsync(r=>r.RoleGUID==role.Id&&r.MenuGUID==leaf.GUID&&r.ApiGUID==id,ct))
                    {db.RoleAccess.Add(new RoleAccess{RoleGUID=role.Id,MenuGUID=leaf.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
            }
        }
    }
}
