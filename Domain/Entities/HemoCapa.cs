using Domain.Attributes;
namespace Domain.Entities;
[ClassName("اقدام اصلاحی و پیشگیرانه")]
public class HemoCapa : AuditedEntity
{
    public Guid HemoReactionGUID { get; set; }
    public string Title { get; set; } = "";
    public string Kind { get; set; } = "";
    public Guid OwnerUserGUID { get; set; }
    public string OwnerName { get; set; } = "";
    public DateTime DueAt { get; set; }
    public string Indicator { get; set; } = "";
    public string Status { get; set; } = "Open";
    public int Progress { get; set; }
    public string? Evidence { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? SubmittedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual HemoReaction Reaction { get; set; } = null!;
    public virtual User Owner { get; set; } = null!;
}
