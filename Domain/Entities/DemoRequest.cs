using Domain.Attributes;

namespace Domain.Entities;

[ClassName("درخواست دمو")]
public class DemoRequest:AuditedEntity
{
    [PropConfig("کاربر ثبت کننده")] public Guid UserGUID { get; set; }
    [PropConfig("پکیج")] public Guid PackageGUID { get; set; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; set; }
    [PropConfig("قبول شده")] public bool? Accepted { get; set; }


    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual Package Package { get; set; } = null!;
    public virtual User User { get; set; } = null!;

}