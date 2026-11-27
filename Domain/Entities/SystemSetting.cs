using Domain.Attributes;

namespace Domain.Entities;


[ClassName("تنظیمات سیستم")]
public class SystemSetting : AuditedEntity
{
    [PropConfig("اعتبار توکن")]
    public int TokenExpireSeconds { get; set; }
    [PropConfig("اعتبار رفرش توکن")]
    public int RefreshTokenExpireSeconds { get; set; }
    [PropConfig("تعداد تلاش ناموق جهت ورود")]
    public int LoginAttempts { get; set; }
    [PropConfig("مدت زمان مسدود سازی کاربر به دلیل تلاش ناموفق برای ورود")]
    public int LockUserSeconds { get; set; }
    [PropConfig("اعتبار توکن")]
    public Guid LoginApiGUID { get; set; }
    
    [PropConfig("ورود همزمان مجاز است")]
    public bool MultiLoginAlowed { get; set; }
    
    [PropConfig("لاگین 2 مرحله ای فعال باشد")]
    public bool TwoStepLogin { get; set; }
    
    
}