using Ardalis.GuardClauses;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

[ClassName("منو")]
public class Menu : BaseEntity
{
    [PropConfig("منو پدر")] public Guid? PMenuGUID { get; set; }

    [PropConfig("عنوان")] public string label { get; set; }

    [PropConfig("آیکون")] public string? icon { get; set; }

    [PropConfig("آدرس",HasIndex:true,IsUnique:true)] public string? routerLink { get; set; }

    [PropConfig("ترتیب نمایش")] public int Gorder { get; set; }
    [PropConfig("نمایش در منو")] public bool ShowInMenu { get; set; }
    
    [PropConfig("منو عمومی می باشد")] public bool IsPublic { get; set; }

    [PropConfig("مسیر", AddToDTO: false, MaxLenght: 128, IsUnique: true)] public string Path { get; set; }
    [PropConfig("سطح", AddToDTO: false, HasIndex: true)] public int Level { get; set; }
    [PropConfig("کد", IsUnique = true, AddToDTO = false)] public int Gkey { get; set; }

    public virtual Menu? PMenu { get; set; } = null!;
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
    public virtual ICollection<MenuApi> MenuApis { get; set; } = new List<MenuApi>();
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

    private async Task SetGkey(DbContext Context)
    {
        var MaxGkey = 0;
        try
        {
            MaxGkey = Context.Set<Menu>().IgnoreQueryFilters().Max(a => a.Gkey);
        }
        catch (Exception e)
        {
        }

        MaxGkey++;

        Gkey = MaxGkey;
    }

    private async Task SetPath(DbContext Context)
    {
        Menu? PMenu;
        if (PMenuGUID == null)
        {
            Path = "/" + Gkey + "/";
        }
        else
        {
            PMenu = Context.Set<Menu>().IgnoreQueryFilters().First(a => a.GUID == PMenuGUID);
            Guard.Against.NotFound((Guid)PMenuGUID, PMenu);
            Path = PMenu.Path + Gkey + "/";
        }

        int _Gorder = 0;
        try
        {
            _Gorder = Context.Set<Menu>().IgnoreQueryFilters().Where(a => a.PMenuGUID == PMenuGUID)
                .Max(a => a.Gorder);
        }
        catch (Exception e)
        {
            _Gorder = 0;
        }

        _Gorder++;

        Level = Path.Count(c => c == '/') - 1;
    }
}