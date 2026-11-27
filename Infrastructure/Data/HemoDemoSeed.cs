using Application.Repository.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Data;

// Explicitly opt-in development seed. No shared default passwords or changes to existing accounts.
public static class HemoDemoSeed
{
    public sealed record Account(string Key,string Username,string RoleName,string FamilyName,string[] Permissions);
    private static readonly string[] ReadKeys=["stats","list","detail","find","history","reviews"];
    public static readonly Account[] Accounts=[
        new("Admission","hemo.admission","HEMO | کارشناس پذیرش","پذیرش",[..ReadKeys,"create","update","review"]),
        new("Supervisor","hemo.supervisor","HEMO | مسئول پذیرش","مسئول پذیرش",[..ReadKeys,"create","update","review","identity"]),
        new("Viewer","hemo.viewer","HEMO | مشاهده پذیرش","مشاهده",ReadKeys)
    ];
    public static readonly (string Key,string Method,string Path)[] Endpoints=[
        ("stats","post","/Identity/PatientAdmission/Stats"),("list","post","/Identity/PatientAdmission/BindPaging"),
        ("detail","get","/Identity/PatientAdmission/{guid}"),("find","get","/Identity/PatientAdmission/FindPatient"),
        ("history","get","/Identity/PatientAdmission/{guid}/History"),("reviews","get","/Identity/PatientAdmission/{guid}/Reviews"),
        ("create","post","/Identity/PatientAdmission"),("update","put","/Identity/PatientAdmission"),
        ("review","post","/Identity/PatientAdmission/Review"),("identity","put","/Identity/PatientAdmission/PatientIdentity")
    ];
    private const string Prefix="HEMO-DEMO-ST02:";
    public static async Task SeedHemoDemoAsync(this WebApplication app)
    {
        if(!app.Configuration.GetValue<bool>("HemoSeed:Enabled"))return;
        if(!app.Environment.IsDevelopment())throw new InvalidOperationException("HEMO demo seed is restricted to Development.");
        using var scope=app.Services.CreateScope();
        await ApplyAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),app.Configuration,CancellationToken.None);
    }
    public static async Task ApplyAsync(ApplicationDbContext db,IConfiguration config,CancellationToken ct)
    {
        var pending=new Dictionary<string,string>();
        foreach(var account in Accounts)
        {
            var existing=await db.User.IgnoreQueryFilters().SingleOrDefaultAsync(u=>u.Username==account.Username,ct);
            if(existing!=null && (existing.IsDelete || existing.ConvertID!=Prefix+account.Key))
                throw new InvalidOperationException("Existing username conflicts with HEMO seed: "+account.Username);
            if(existing==null)
            {
                var password=config["HemoSeed:Passwords:"+account.Key];
                if(string.IsNullOrWhiteSpace(password)||password.Length<16||password.Length>256 ||
                   await db.PasswordBlockList.AnyAsync(p=>p.Password==password,ct))
                    throw new InvalidOperationException("Supply a unique password of 16+ characters for "+account.Key);
                pending.Add(account.Key,password);
            }
        }
        if(pending.Values.Distinct(StringComparer.Ordinal).Count()!=pending.Count)
            throw new InvalidOperationException("HEMO seed passwords must be distinct.");
        await using var tx=await db.Database.BeginTransactionAsync(ct);
        var root=await db.Role.SingleOrDefaultAsync(r=>r.Gkey==1,ct);
        if(root==null)
        {
            if(await db.Role.IgnoreQueryFilters().AnyAsync(ct))throw new InvalidOperationException("Existing identity root role is missing.");
            root=new Role{Name="نقش ریشه (ساختاری)",Path="/1/",Gkey=1,Level=1};
            db.Role.Add(root);await db.SaveChangesAsync(ct);
        }
        var org=await db.OrgLocation.SingleOrDefaultAsync(o=>o.ConvertID==Prefix+"Facility",ct);
        if(org==null)
        {
            org=new OrgLocation{Name="بیمارستان آزمایشی HEMO",ConvertID=Prefix+"Facility"};
            db.OrgLocation.Add(org);await db.SaveChangesAsync(ct);
        }
        // Reuse the identity subsystem; only create its metadata on an otherwise empty demo database.
        var identityGuid=Guid.Parse("41e8f63b-ac36-44bb-9bae-2d3355b1b743");
        var subsystem=await db.SubSystem.SingleOrDefaultAsync(s=>s.GUID==identityGuid,ct);
        if(subsystem==null)
        {
            if(await db.SubSystem.AnyAsync(ct))throw new InvalidOperationException("Identity subsystem must already be registered.");
            var url=new Uri(config["HemoSeed:ApiBaseUrl"]??"http://127.0.0.1:1002");
            subsystem=new SubSystem{GUID=identityGuid,Name="HEMO Identity",Host=url.Host,Port=url.Port,Active=true};
            db.SubSystem.Add(subsystem);await db.SaveChangesAsync(ct);
        }
        if(!subsystem.Active)throw new InvalidOperationException("Identity subsystem is inactive.");
        var menu=await db.Menu.SingleOrDefaultAsync(m=>m.routerLink=="/admission",ct);
        if(menu==null)
        {
            menu=new Menu{label="پذیرش بیمارستان",routerLink="/admission",icon="pi pi-id-card",ShowInMenu=true,Gorder=1,IsPublic=false};
            db.Menu.Add(menu);await db.SaveChangesAsync(ct);
        }
        var apis=new Dictionary<string,Guid>();
        foreach(var e in Endpoints.Concat(new[]{("login","post","/Identity/User/Login"),("hemo-login","post","/Identity/HemoSession/Login")}))
        {
            var api=await db.Api.FirstOrDefaultAsync(a=>a.SubSystemGUID==subsystem.GUID && a.APITypeName==e.Item2 && a.DownstreamPathTemplate==e.Item3,ct);
            if(api==null)
            {
                api=new Api{SubSystemGUID=subsystem.GUID,APITypeName=e.Item2,DownstreamPathTemplate=e.Item3,UpstreamPathTemplate=e.Item3,Title="HEMO "+e.Item1,Active=true,IsPublic=false};
                db.Api.Add(api);await db.SaveChangesAsync(ct);
            }
            if(!api.Active)throw new InvalidOperationException("HEMO API is inactive: "+e.Item3);
            apis[e.Item1]=api.GUID;
            if(e.Item1 is "login" or "hemo-login")continue;
            if(!await db.MenuApi.AnyAsync(m=>m.MenuGUID==menu.GUID&&m.ApiGUID==api.GUID,ct))
            {db.MenuApi.Add(new MenuApi{MenuGUID=menu.GUID,ApiGUID=api.GUID});await db.SaveChangesAsync(ct);}
        }
        foreach(var account in Accounts)
        {
            var role=await db.Role.SingleOrDefaultAsync(r=>r.ConvertID==Prefix+account.Key,ct);
            if(role==null)
            {
                role=new Role{Name=account.RoleName,PRoleGUID=root.GUID,OrgLocationGUID=org.GUID,ConvertID=Prefix+account.Key};
                db.Role.Add(role);await db.SaveChangesAsync(ct);
            }
            foreach(var key in account.Permissions)
            {
                var id=apis[key];
                if(!await db.RoleAccess.AnyAsync(r=>r.RoleGUID==role.GUID&&r.MenuGUID==menu.GUID&&r.ApiGUID==id,ct))
                {db.RoleAccess.Add(new RoleAccess{RoleGUID=role.GUID,MenuGUID=menu.GUID,ApiGUID=id});await db.SaveChangesAsync(ct);}
            }
            var user=await db.User.SingleOrDefaultAsync(u=>u.Username==account.Username,ct);
            if(user==null)
            {
                user=new User{Username=account.Username,FirstName="کاربر آزمایشی",LastName=account.FamilyName,ConvertID=Prefix+account.Key,MobileIsConfirm=true,ForceChangePassword=false};
                user.Password=UserService.HashPassword(user.GUID,pending[account.Key]);
                db.User.Add(user);await db.SaveChangesAsync(ct);
            }
            if(!await db.UserRole.AnyAsync(r=>r.UserGUID==user.GUID&&r.RoleGUID==role.GUID&&r.OrgLocationGUID==org.GUID,ct))
            {
                db.UserRole.Add(new UserRole{UserGUID=user.GUID,RoleGUID=role.GUID,OrgLocationGUID=org.GUID,ExpireDate=DateTime.Now.AddYears(1)});
                await db.SaveChangesAsync(ct);
            }
        }
        await tx.CommitAsync(ct);
    }
}
