using Domain.Attributes;

namespace Domain.Entities;

[ClassName("لیست ای پی عا بلاک شده")]
public class BlockListIP:AuditedEntity
{
    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; set; }
}