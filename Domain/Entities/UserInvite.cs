using Domain.Attributes;

namespace Domain.Entities;

[ClassName("دعوت نامه")]
public class UserInvite: AuditedEntity
{
    [PropConfig("کاربر")]
    public Guid UserGUID { get; set; }
    [PropConfig("شرکت")]
    public Guid OrgLocationGUID { get; set; }
    [PropConfig("قبول کردن کاربر")]
    public bool? UserAccepted { get; set; }
    [PropConfig("قبول کردن شرکت")]
    public bool? OrgLocationAccepted { get; set; }
    [PropConfig("کاربر")]
    public string UserInfo { get; set; }
    
    public virtual User User { get; set; } = null!;
    public virtual OrgLocation OrgLocation { get; set; } = null!;
}