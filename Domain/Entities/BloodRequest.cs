using Domain.Attributes;
namespace Domain.Entities;
[ClassName("درخواست خون")]
public class BloodRequest : AuditedEntity
{
    public Guid PatientAdmissionGUID { get; set; }
    public Guid ClientRequestId { get; set; }
    public string RequestNumber { get; set; } = "";
    public Guid RequesterUserGUID { get; set; }
    public Guid RequesterRoleGUID { get; set; }
    public string Source { get; set; } = "Nurse";
    public string FormType { get; set; } = "General";
    public string Priority { get; set; } = "Routine";
    public string Status { get; set; } = "Draft";
    public int SubmissionAttempt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public byte[]? AdmissionVersionAtSubmission { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual PatientAdmission PatientAdmission { get; set; } = null!;
    public virtual ICollection<BloodRequestItem> Items { get; set; } = new List<BloodRequestItem>();
    public virtual RequestClinicalData? ClinicalData { get; set; }
    public virtual ICollection<RequestApproval> Approvals { get; set; } = new List<RequestApproval>();
}
