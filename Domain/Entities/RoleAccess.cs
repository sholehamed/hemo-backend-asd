using Ardalis.GuardClauses;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[ClassName("دسترسی نقش")]
public class RoleAccess : AuditedEntity
{
    [PropConfig("منو")] public Guid MenuGUID { get; set; }
    [PropConfig("نقش")] public Guid RoleGUID { get; set; }
    [PropConfig("سرویس")] public Guid? ApiGUID { get; set; }
    [PropConfig("دسترسی پدر")] public Guid? PRoleAccessGUID { get; set; }
    
    [PropConfig("دریافت شده از پکیج")] public Guid? OrglocationPackageGUID { get; set; }

    [PropConfig("مسیر", AddToDTO: false, MaxLenght: 128, IsUnique: true)]
    public string Path { get; set; }

    [PropConfig("سطح", AddToDTO: false, HasIndex: true)]
    public int Level { get; set; }

    [PropConfig("کد", IsUnique = true, AddToDTO = false)]
    public int Gkey { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual OrglocationPackage OrglocationPackage { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
    public virtual Api Api { get; set; } = null!;

    public override async Task OnAdd(DbContext Context)
    {
        await SetGkey(Context);
        await SetPath(Context);
    }

    private async Task SetGkey(DbContext Context)
    {
        var MaxGkey = 0;
        try
        {
            MaxGkey = Context.Set<RoleAccess>().IgnoreQueryFilters().Max(a => a.Gkey);
        }
        catch (Exception e)
        {
        }

        MaxGkey++;

        Gkey = MaxGkey;
    }

    private async Task SetPath(DbContext Context)
    {
        RoleAccess? _PRoleAccess;
        if (PRoleAccessGUID == null)
        {
            Path = "/" + Gkey + "/";
        }
        else
        {
            _PRoleAccess = Context.Set<RoleAccess>().IgnoreQueryFilters().First(a => a.GUID == PRoleAccessGUID);
            Guard.Against.NotFound((Guid)PRoleAccessGUID, _PRoleAccess);
            Path = _PRoleAccess.Path + Gkey + "/";
        }

        Level = Path.Count(c => c == '/') - 1;
    }
}