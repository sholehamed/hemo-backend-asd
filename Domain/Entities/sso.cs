using Domain.Attributes;

namespace Domain.Entities;

[ClassName("SSO")]
public class sso : BaseEntity
{
    [PropConfig("کاربر")] public Guid UserGUID { get; set; }
    [PropConfig("پکیج")] public Guid PackageGUID { get; set; }
    [PropConfig("شرکت")] public Guid OrgLocationGUID { get; set; }
    [PropConfig("آدرس IP")] public string IPAddress { get; set; }
    [PropConfig("مرورگر")] public string UserAgent { get; set; }
    [PropConfig("تاریخ انقضا")] public DateTime ExpireDate { get; set; }
    [PropConfig("کلید", IsUnique: true)] public string Key { get; set; }
    [PropConfig("اعتبارسنجی")] public string Sign { get; set; }
    [PropConfig("ورود به عنوان ادمین کاربران")] public bool AdminLogin { get; set; }
}