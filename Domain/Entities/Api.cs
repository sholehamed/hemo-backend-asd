using Domain.Attributes;

namespace Domain.Entities;

[ClassName("سرویس")]
public class Api : AuditedEntity
{
    [PropConfig("مسیر درخواست ارسالی",MaxLenght:256)] public string DownstreamPathTemplate { get; set; }

    [PropConfig("مسیر درخواست دریافتی",MaxLenght:256)] public string UpstreamPathTemplate { get; set; }

    [PropConfig("سرویس از نوع عمومی می باشد")]
    public bool IsPublic { get; set; }

    [PropConfig("اولویت")] public int Priority { get; set; }

    [PropConfig("زیر سیستم")] public Guid SubSystemGUID { get; set; }

    [PropConfig("متد درخواست")] public string APITypeName { get; set; }

    [PropConfig("فعال")] public bool Active { get; set; }
    [PropConfig("عنوان")] public string Title { get; set; }

    [PropConfig("قابل نمایش در اینترنت")]
    public bool Internet { get; set; }
    [PropConfig("قابل نمایش در اینترانت")]
    public bool Intranet { get; set; }
    
    [PropConfig("لاگ فعال باشد")]
    public bool LogData { get; set; }

    [PropConfig("اطلاعات حساس")]
    public string? Sensitive { get; set; }
    
    [PropConfig("هدر درخواست کاربر لاگ شود")]
    public bool LogRequestHeaders { get; set; }
    [PropConfig("هدر پاسخ سرویس لاگ شود")]
    public bool LogResponseHeaders { get; set; }
    [PropConfig("بدنه درخواست کاربر لاگ شود")]
    public bool LogRequestBody { get; set; }
    [PropConfig("بدنه پاسخ سرویس لاگ شود")]
    public bool LogResponseBody { get; set; }
    
    [PropConfig("ریت لیمیت فعال باشد")]
    public bool EnableRateLimiting { get; set; } = false;
    [PropConfig("مدت زمان دوره")]
    public int? Period { get; set; } = null;
    [PropConfig("تعداد درخواست مجاز")]
    public int? Limit { get; set; }
    
    
    public virtual SubSystem SubSystem { get; set; } = null!;
    public virtual ICollection<MenuApi> MenuApis { get; set; } = new List<MenuApi>();
    public virtual ICollection<RoleAccess> RoleAccess { get; set; } = new List<RoleAccess>();
}