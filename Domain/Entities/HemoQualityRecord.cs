using Domain.Attributes;
namespace Domain.Entities;
[ClassName("پرونده مدیریت کیفیت")]
public class HemoQualityRecord : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public string Kind { get; set; } = "";
    public string Number { get; set; } = "";
    public string Title { get; set; } = "";
    public string Ward { get; set; } = "";
    public string Status { get; set; } = "Draft";
    public DateTime OccurredAt { get; set; }
    public Guid CreatedByUserGUID { get; set; }
    public string DetailsJson { get; set; } = "{}";
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual OrgLocation OrgLocation { get; set; } = null!;
}
