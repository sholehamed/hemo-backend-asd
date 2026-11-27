using Domain.Attributes;
namespace Domain.Entities;
[ClassName("حادثه تجهیز")]
public class HemoEquipmentIncident : AuditedEntity
{
    public Guid HemoEquipmentGUID { get; set; }
    public string IncidentNumber { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string Status { get; set; } = "Open";
    public DateTime ReportedAt { get; set; }
    public Guid ReporterUserGUID { get; set; }
    public string Description { get; set; } = "";
    public string? Resolution { get; set; }
    public string? ResolutionReference { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual HemoEquipment Equipment { get; set; } = null!;
}
