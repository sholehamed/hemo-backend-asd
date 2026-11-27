using Domain.Attributes;

namespace Domain.Entities;

[ClassName("کاربر")]
public class User : AuditedEntity
{
    [PropConfig("نام کاربری", IsUnique: true, MaxLenght: 64)]
    public string? Username { get; set; }
    [PropConfig("رمز عبور")] public string Password { get; set; }
    [PropConfig("نام")] public string FirstName { get; set; }
    [PropConfig("نام خانوادگی")] public string LastName { get; set; }

    [PropConfig("موبایل", MaxLenght: 16)]
    public string? Mobile { get; set; }

    [PropConfig("ایمیل", IsUnique: true, MaxLenght: 64)]
    public string? Email { get; set; }
    
    [PropConfig("کد ملی", MaxLenght: 10)]
    public string? Nationalcode { get; set; }
    
    
    [PropConfig("حساب کاربری مسدود شده است")]
    public bool Blocked { get; set; }
    
    [PropConfig("حساب کاربری قفل شده است")]
    public bool Locked { get; set; }
    [PropConfig("تاریخ اتمام محدودیت ورود")]
    public DateTime? LockoutEndDate { get; set; }
    
    [PropConfig("تعداد تلاش نا موفق برای ورود")]
    public int FailedLoginCount { get; set; }
    
    [PropConfig("موبایل تایید شده است")]
    public bool MobileIsConfirm { get; set; }
    [PropConfig("تغییر رمز عبور اجباری می باشد")] public bool ForceChangePassword { get; set; }
  
    [PropConfig("تاریخ تولد")]
    public DateTime? BirthDate { get; set; }
    
    [PropConfig("نام پدر")]
    public string? FatherName { get; set; }
    
    [PropConfig("آقا می باشد")]
    public bool? IsMan { get; set; }
    
    public virtual ICollection<DemoRequest> DemoRequests { get; set; } = new List<DemoRequest>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserInvite> UserInvites { get; set; } = new List<UserInvite>();
    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
    public virtual ICollection<OrgLocation> OrgLocations { get; set; } = new List<OrgLocation>();
}