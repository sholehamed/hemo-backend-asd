using Ardalis.GuardClauses;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[ClassName("شرکت")]
public class OrgLocation : AuditedEntity
{
    [PropConfig("صاحب شرکت")] public Guid? UserGUID { get; set; }
    [PropConfig("نوع شرکت")] public Guid? OrgLocationTypeGUIO { get; set; }
    [PropConfig("عنوان")] public string Name { get; set; }
    [PropConfig("شرکت پدر")] public Guid? POrgLocationGUID { get; set; }
    [PropConfig("آدرس")] public string? Address { get; set; }
    [PropConfig("تلفن")] public string? Tell { get; set; }
    [PropConfig("فکس")] public string? Fax { get; set; }
    [PropConfig("ایمیل")] public string? Email { get; set; }
    [PropConfig("آدرس وبسایت")] public string? Url { get; set; }

    [PropConfig("کد", IsUnique = true, AddToDTO = false)]
    public int Gkey { get; set; }

    [PropConfig("مسیر", AddToDTO: false, MaxLenght: 128, IsUnique: true)]
    public string Path { get; set; }

    [PropConfig("سطح", AddToDTO: false, HasIndex: true)]
    public int Level { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual OrgLocation POrgLocation { get; set; } = null!;

    public virtual ICollection<OrglocationPackage> OrglocationPackages { get; set; } = new List<OrglocationPackage>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<OrgLocation> ChildOrgLocations { get; set; } = new List<OrgLocation>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserInvite> UserInvites { get; set; } = new List<UserInvite>();
    public virtual ICollection<OrgType> OrgTypes { get; set; } = new List<OrgType>();
    public virtual OrgLocationType? OrgLocationType { get; set; } = null!;

    public virtual ICollection<ShoppingCartProduct> ShoppingCartProducts { get; set; } =
        new List<ShoppingCartProduct>();

    public override async Task OnAdd(DbContext Context)
    {
        await SetGkey(Context);
        await SetPath(Context);
    }

    public async Task SetGkey(DbContext context)
    {
        var MaxGkey = 0;
        try
        {
            MaxGkey = context.Set<OrgLocation>().IgnoreQueryFilters().Max(a => a.Gkey);
        }
        catch (Exception e)
        {
        }

        MaxGkey++;

        Gkey = MaxGkey;
    }

    public async Task SetPath(DbContext context)
    {
        OrgLocation? POrgLocation;
        if (POrgLocationGUID == null)
        {
            Path = "/" + Gkey + "/";
        }
        else
        {
            POrgLocation = context.Set<OrgLocation>().Find(POrgLocationGUID);
            Guard.Against.NotFound((Guid)POrgLocationGUID, POrgLocation);
            Path = POrgLocation.Path + Gkey + "/";
        }

        Level = Path.Count(c => c == '/') - 1;
    }
}