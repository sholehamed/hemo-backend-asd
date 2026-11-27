using Domain.Attributes;
namespace Domain.Entities;
[ClassName("ردیف پرونده کیفیت")]
public class HemoQualityLine : AuditedEntity
{
    public Guid HemoQualityRecordGUID { get; set; }
    public string Title { get; set; } = "";
    public string DetailsJson { get; set; } = "{}";
    public string Status { get; set; } = "Draft";
    public Guid? OwnerUserGUID { get; set; }
    public DateTime? DueAt { get; set; }
    public int Progress { get; set; }
    public string? Evidence { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual HemoQualityRecord Record { get; set; } = null!;
}
