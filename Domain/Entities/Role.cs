using Ardalis.GuardClauses;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[ClassName("نقش")]
public class Role : AuditedEntity
{
    [PropConfig("نوع شرکت")] public Guid? OrgLocationTypeGUID { get; set; }
    [PropConfig("بالا دست")] public Guid? PRoleGUID { get; set; }
    [PropConfig("عنوان")] public string Name { get; set; }
    [PropConfig("شرکت")] public Guid? OrgLocationGUID { get; set; }

    [PropConfig("مسیر", AddToDTO: false, MaxLenght: 128, IsUnique: true)]
    public string Path { get; set; }

    [PropConfig("کد", IsUnique = true, AddToDTO = false)]
    public int Gkey { get; set; }

    [PropConfig("سطح", AddToDTO: false, HasIndex: true)]
    public int Level { get; set; }

    public virtual Role? PRole { get; set; } = null!;
    public virtual OrgLocation? OrgLocation { get; set; } = null;
    public virtual OrgLocationType? OrgLocationType { get; set; } = null!;
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<RoleAccess> RoleAccess { get; set; } = new List<RoleAccess>();

    public override async Task OnAdd(DbContext Context)
    {
        await SetGkey(Context);
        await SetPath(Context);
    }

    public override async Task OnModified(DbContext Context)
    {
        await SetPath(Context);
    }

    public async Task SetGkey(DbContext context)
    {
        var MaxGkey = 0;
        try
        {
            MaxGkey = context.Set<Role>().IgnoreQueryFilters().Max(a => a.Gkey);
        }
        catch (Exception e)
        {
        }

        MaxGkey++;

        Gkey = MaxGkey;
    }

    public async Task SetPath(DbContext context)
    {
        if (Gkey == 1)
        {
            return;
        }
        Role? PRole;
        if (PRoleGUID == null)
        {
            PRole = context.Set<Role>().First(a => a.Gkey == 1);
            PRoleGUID = PRole.GUID;
        }
        else
        {
            PRole = context.Set<Role>().Find(PRoleGUID);
            Guard.Against.NotFound((Guid)PRoleGUID, PRole);
        }

        Path = PRole.Path + Gkey + "/";

        Level = Path.Count(c => c == '/') - 1;
    }
}