using Domain.Attributes;

namespace Domain.Entities;

[ClassName("رمز یکبار مصرف")]
public class OTP:BaseEntity
{
    [PropConfig("نام کاربری",HasIndex = true)]
    public string Username { get; set; }

    [PropConfig("کد یکبار مصرف")]
    public string Code { get; set; }

    [PropConfig("تاریخ انقضا")]
    public DateTime ExDate { get; set; }

    [PropConfig("تعداد تلاش مجاز")]
    public int TryCount { get; set; }
    
    [PropConfig("استفاده شده")]
    public bool Used { get; set; }
}