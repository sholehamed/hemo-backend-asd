using System.ComponentModel.DataAnnotations.Schema;
using Domain.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Domain.Common;

public abstract class BaseEntity:BaseEvent
{
    [PropConfig("کلید")] public Guid GUID { get; set; } = Guid.NewGuid();
    [PropConfig("تاریخ ثبت")] public DateTime Created { get; set; }
    [PropConfig("کد کانورت دیتا")] public string? ConvertID { get; set; }
    
}

public abstract class CreateAuditedEntity : BaseEntity
{
    [PropConfig("ثبت شده توسط")] public Guid CreatedBy { get; set; }
    [PropConfig("ثبت شده توسط IP")] public string CreatedByIP { get; set; }
}

public abstract class ModificationAuditedEntity : CreateAuditedEntity
{
    [PropConfig("اخرین تاریخ ویرایش")] public DateTime LastModified { get; set; }
    [PropConfig("اخرین ویرایش توسط")] public Guid LastModifiedBy { get; set; }
    [PropConfig("اخرین ویرایش توسط IP")] public string LastModifiedByIP { get; set; }
}

public abstract class AuditedEntity : ModificationAuditedEntity
{
    [PropConfig("تاریخ حذف")] public DateTime? Deleted { get; set; }
    [PropConfig("حذف شده توسط")] public Guid? DeletedBy { get; set; }
    [PropConfig("حذف شده توسط IP")] public string? DeletedByIP { get; set; }
    [PropConfig("حذف شده")] public bool IsDelete { get; set; } = false;
}

public abstract class FullAuditedEntity : AuditedEntity
{
    [PropConfig("تاریخ حذف")] public DateTime? Deactivated { get; set; }
    [PropConfig("حذف شده توسط")] public Guid? DeactivatedBy { get; set; }
    [PropConfig("حذف شده توسط IP")] public string? DeactivatedByIP { get; set; }
    [PropConfig("حذف شده")] public bool IsDeactivated { get; set; } = false;
}