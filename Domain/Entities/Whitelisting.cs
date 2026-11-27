using Domain.Attributes;

namespace Domain.Entities;

[ClassName("لیست ای پی ها مجاز")]
public class Whitelisting:AuditedEntity
{
    [PropConfig("آدرس ای پی")]
    public string IPAddress { get; set; }
}