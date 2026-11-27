using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class HemoBankReadRouteSeed
{
    public sealed record Route(string Path,string Label,string Icon,string Required,string[] Mapped,bool Visible=true,string[]? AdditionalRequired=null);
    public static readonly Route[] Routes=[
        new("/hemo/bank/inventory","موجودی بانک خون","pi pi-inbox","bank-inventory",["bank-inventory"]),
        new("/hemo/bank/reports","گزارش‌های بانک خون","pi pi-chart-bar","bank-stats",["bank-stats","bank-inventory","bank-temperature-history"])
    ];
    public static readonly Route[] UnitRoutes=[
        new("/hemo/bank/receive","ثبت دریافت واحد","pi pi-plus","bank-receive",["bank-receive"],true,["bank-unit"]),
        new("/hemo/bank/unit/:guid","جزئیات و عملیات واحد","pi pi-file","bank-unit",["bank-unit","bank-inspect","bank-test","bank-reserve","bank-thaw","bank-ready","bank-hold","bank-unreserve","bank-discard","request-bank","request-bank-detail"],false)
    ];
    public static bool CanOpen(Route route,IEnumerable<string> keys)
    {
        var allowed=keys.ToHashSet(StringComparer.Ordinal);
        return allowed.Contains(route.Required)&&(route.AdditionalRequired?.All(allowed.Contains)??true);
    }
    internal static async Task ApplyAsync(ApplicationDbContext db,Menu original,IReadOnlyDictionary<string,Guid> apis,
        IEnumerable<HemoDemoSeed.Account> accounts,CancellationToken ct,bool includeUnitPages=false)
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
        if(original.IsPublic||!original.ShowInMenu)throw new InvalidOperationException("Existing bank menu is not private and visible.");
        if(original.PMenuGUID!=root.GUID)
        {
            var ids=HemoDemoSeed.BankEndpoints.Select(e=>apis[e.Key]).ToArray();
            if(original.PMenuGUID!=null||original.ConvertID!=null||original.label!="مدیریت بانک خون"||original.icon!="pi pi-inbox"||original.Gorder!=3||
                await db.Menu.AnyAsync(m=>m.PMenuGUID==original.GUID,ct)||
                await db.MenuApi.AnyAsync(m=>m.MenuGUID==original.GUID&&!ids.Contains(m.ApiGUID),ct))
                throw new InvalidOperationException("Existing bank menu conflicts with R02 demo promotion.");
            original.PMenuGUID=root.GUID;await db.SaveChangesAsync(ct);
            if(original.Path!=root.Path+original.Gkey+"/")throw new InvalidOperationException("Bank menu path was not recalculated.");
        }
        var request=await db.Menu.SingleAsync(m=>m.routerLink=="/hemo/requests",ct);
        var roles=new List<(Guid Id,HashSet<string> Keys)>();
        foreach(var account in accounts)
        {
            var role=await db.Role.SingleAsync(r=>r.ConvertID=="HEMO-DEMO-ST02:"+account.Key,ct);
            var grants=(await db.RoleAccess.Where(r=>r.RoleGUID==role.GUID&&(r.MenuGUID==original.GUID||r.MenuGUID==request.GUID)&&r.ApiGUID.HasValue)
                .Select(r=>r.ApiGUID!.Value).ToListAsync(ct)).ToHashSet();
            roles.Add((role.GUID,account.Permissions.Where(k=>apis.TryGetValue(k,out var id)&&grants.Contains(id)).ToHashSet(StringComparer.Ordinal)));
        }
        foreach(var route in includeUnitPages?Routes.Concat(UnitRoutes):Routes)
        {
            var marker="HEMO-R02-BANK:"+route.Path;
            var leaf=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink==route.Path,ct);
            if(leaf==null)
            {
                leaf=new Menu{routerLink=route.Path,label=route.Label,icon=route.Icon,ShowInMenu=route.Visible,IsPublic=false,PMenuGUID=root.GUID,ConvertID=marker};
                db.Menu.Add(leaf);await db.SaveChangesAsync(ct);
            }
            else if(leaf.ConvertID!=marker||leaf.IsPublic||leaf.ShowInMenu!=route.Visible||leaf.PMenuGUID!=root.GUID)
                throw new InvalidOperationException("Existing bank route conflicts with R02 seed: "+route.Path);
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
