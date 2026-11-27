using Domain.Attributes;
namespace Domain.Entities;
[ClassName("رویداد حواله تزریق")]
public class BloodTransferEvent : AuditedEntity
{
    public Guid BloodTransferGUID { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public string Action { get; set; } = "";
    public string PreviousStatus { get; set; } = "";
    public string NewStatus { get; set; } = "";
    public DateTime OccurredAt { get; set; }
    public string? Notes { get; set; }
    public string? EvidenceJson { get; set; }
    public virtual BloodTransfer Transfer { get; set; } = null!;
}
