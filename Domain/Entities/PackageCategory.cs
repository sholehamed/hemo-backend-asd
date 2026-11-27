using Domain.Attributes;

namespace Domain.Entities;

[ClassName("دسته بندی پکیج")]
public class PackageCategory : AuditedEntity
{
    [PropConfig("عنوان")] public string Name { get; set; }
    [PropConfig("ترتیب نمایش")] public int Gorder { get; set; }
    
    [PropConfig("آیکون")]
    public string Icon { get; set; }

    [PropConfig("توضیحات", MaxLenght: 1024)]
    public string? Description { get; set; }

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();
}