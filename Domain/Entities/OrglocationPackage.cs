using Domain.Attributes;

namespace Domain.Entities;

[ClassName("پکیج های شرکت")]
public class OrglocationPackage:AuditedEntity
{
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; set; }
    [PropConfig("پکیج")] public Guid PackageGUID { get; set; }
    [PropConfig("اعتبار")] public DateTime ExpiryDate { get; set; }
    [PropConfig("پکیج پنهان باشد")] public bool Hidden { get; set; }
   [PropConfig("درخواست دمو")] public Guid? DemoRequestGUID { get; set; }

    public virtual DemoRequest? DemoRequest { get; set; } 

    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual Package Package { get; set; } = null!;
}