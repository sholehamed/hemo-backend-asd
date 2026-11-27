using Domain.Attributes;
namespace Domain.Entities;
[ClassName("گزارش دوره‌ای هموویژیلانس")]
public class HemoReport : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public string ReportNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime FromAt { get; set; }
    public DateTime ToAt { get; set; }
    public string? Ward { get; set; }
    public string? ProductCode { get; set; }
    public Guid PreparedBy { get; set; }
    public DateTime PreparedAt { get; set; }
    public string SnapshotJson { get; set; } = "{}";
    public string ExternalStatus { get; set; } = "NotRecorded";
    public string? ExternalReference { get; set; }
    public Guid? ExternalRecordedBy { get; set; }
    public DateTime? ExternalRecordedAt { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual OrgLocation OrgLocation { get; set; } = null!;
}
