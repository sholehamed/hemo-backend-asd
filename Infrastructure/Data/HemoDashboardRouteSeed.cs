using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class HemoDashboardRouteSeed
{
    public const string Path="/hemo/dashboard";
    public static readonly string[] StatsKeys=["stats","request-stats","bank-stats","tx-stats","hv-stats","quality-equipment-stats","quality-stats"];
    public static bool CanOpen(IEnumerable<string> keys)=>keys.Any(StatsKeys.Contains);
    internal static async Task ApplyAsync(ApplicationDbContext db,IReadOnlyDictionary<string,Guid> apis,
        IReadOnlyDictionary<string,Guid> originalMenus,IEnumerable<HemoDemoSeed.Account> accounts,CancellationToken ct)
    {
        const string rootMarker="HEMO-R02-COMPANY-MENU",marker="HEMO-R02-DASHBOARD";
        var root=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/Hemo",ct);
        if(root==null)
        {
            root=new Menu{routerLink="/Hemo",label="هموویژیلانس",icon="pi pi-heart",ShowInMenu=true,IsPublic=false,ConvertID=rootMarker};
            db.Menu.Add(root);await db.SaveChangesAsync(ct);
        }
        else if(root.ConvertID!=rootMarker||root.PMenuGUID!=null||root.IsPublic||!root.ShowInMenu)
            throw new InvalidOperationException("Existing /Hemo menu is not owned by demo seed.");
        var menu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink==Path,ct);
        if(menu==null)
        {
            menu=new Menu{routerLink=Path,label="داشبورد مدیریت بیمارستان",icon="pi pi-chart-pie",ShowInMenu=true,IsPublic=false,PMenuGUID=root.GUID,ConvertID=marker,Gorder=0};
            db.Menu.Add(menu);await db.SaveChangesAsync(ct);
        }
        else if(menu.ConvertID!=marker||menu.IsPublic||!menu.ShowInMenu||menu.PMenuGUID!=root.GUID)
            throw new InvalidOperationException("Existing dashboard menu conflicts with R02 seed.");
        var mapped=StatsKeys.Select(k=>apis[k]).ToArray();
        if(await db.MenuApi.AnyAsync(x=>x.MenuGUID==menu.GUID&&!mapped.Contains(x.ApiGUID),ct)||
           await db.RoleAccess.AnyAsync(x=>x.MenuGUID==menu.GUID&&(!x.ApiGUID.HasValue||!mapped.Contains(x.ApiGUID.Value)),ct))
            throw new InvalidOperationException("Dashboard menu has non-aggregate or whole-menu grants.");
        foreach(var id in mapped)
            if(!await db.MenuApi.AnyAsync(x=>x.MenuGUID==menu.GUID&&x.ApiGUID==id,ct))
            {db.MenuApi.Add(new MenuApi{MenuGUID=menu.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
        foreach(var account in accounts.Where(a=>CanOpen(a.Permissions)))
        {
            var role=await db.Role.SingleAsync(x=>x.ConvertID=="HEMO-DEMO-ST02:"+account.Key,ct);
            foreach(var key in StatsKeys.Where(account.Permissions.Contains))
            {
                var id=apis[key];var original=originalMenus[key];
                if(!await db.RoleAccess.AnyAsync(x=>x.RoleGUID==role.GUID&&x.MenuGUID==original&&x.ApiGUID==id,ct))continue;
                if(!await db.RoleAccess.AnyAsync(x=>x.RoleGUID==role.GUID&&x.MenuGUID==menu.GUID&&x.ApiGUID==id,ct))
                {db.RoleAccess.Add(new RoleAccess{RoleGUID=role.GUID,MenuGUID=menu.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
            }
        }
    }
}
