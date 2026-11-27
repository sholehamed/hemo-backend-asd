using Domain.Attributes;

namespace Domain.Entities;

[ClassName("پکیج های نرم افزاری")]
public class Package : AuditedEntity
{
    [PropConfig("دسته بندی")] public Guid PackageCategoryGUID { get; set; }
    [PropConfig("عنوان")] public string Name { get; set; }
    [PropConfig("عنوان فرعی")] public string Subtitle { get; set; }
    [PropConfig("توضیحات")] public string Description { get; set; }
    [PropConfig("مبلغ")] public long Price { get; set; }
    [PropConfig("لینک")] public string Link { get; set; }
    [PropConfig("سطح دسترسی")] public Guid RoleGUID { get; set; }
    [PropConfig("آیکون")] public string Icon { get; set; }
    [PropConfig("ترتیب نمایش")] public int Gorder { get; set; }
    [PropConfig("تعداد روز اعتبار")] public int ExpireDays { get; set; }
    [PropConfig("منو اصلی")] public Guid? MainMenuGuid { get; set; }
    [PropConfig("درصد تخفیف تمدید")] public int RenewDiscount { get; set; }
    
    public virtual PackageCategory PackageCategory { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual Menu MainMenu { get; set; } = null!;
    
    public virtual ICollection<OrglocationPackage> OrglocationPackages { get; set; } = new List<OrglocationPackage>();
}