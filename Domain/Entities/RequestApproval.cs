using Domain.Attributes;
namespace Domain.Entities;
[ClassName("تاریخچه تصمیم درخواست")]
public class RequestApproval : AuditedEntity
{
    public Guid BloodRequestGUID { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public string Action { get; set; } = "";
    public string PreviousStatus { get; set; } = "";
    public string NewStatus { get; set; } = "";
    public string? Reason { get; set; }
    public int SubmissionAttempt { get; set; }
    public DateTime OccurredAt { get; set; }
    public virtual BloodRequest BloodRequest { get; set; } = null!;

}
