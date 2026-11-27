using Domain.Attributes;
namespace Domain.Entities;
[ClassName("رویداد تجهیز")]
public class HemoEquipmentEvent : AuditedEntity
{
    public Guid HemoEquipmentGUID { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public string Kind { get; set; } = "";
    public string PreviousStatus { get; set; } = "";
    public string NewStatus { get; set; } = "";
    public DateTime OccurredAt { get; set; }
    public string Notes { get; set; } = "";
    public string? Reference { get; set; }
    public string? Provider { get; set; }
    public DateTime? DueAt { get; set; }
    public string? EvidenceJson { get; set; }
    public virtual HemoEquipment Equipment { get; set; } = null!;
}
