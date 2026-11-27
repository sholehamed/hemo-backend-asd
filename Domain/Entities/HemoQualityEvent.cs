using Domain.Attributes;
namespace Domain.Entities;
[ClassName("رویداد پرونده کیفیت")]
public class HemoQualityEvent : AuditedEntity
{
    public Guid HemoQualityRecordGUID { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public string Action { get; set; } = "";
    public string PreviousStatus { get; set; } = "";
    public string NewStatus { get; set; } = "";
    public DateTime OccurredAt { get; set; }
    public string Notes { get; set; } = "";
    public string? EvidenceJson { get; set; }
    public virtual HemoQualityRecord Record { get; set; } = null!;
}
