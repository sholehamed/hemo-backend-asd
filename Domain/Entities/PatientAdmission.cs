using Domain.Attributes;
namespace Domain.Entities;
[ClassName("پذیرش بیمار")]
public class PatientAdmission : AuditedEntity
{
    public Guid PatientGUID { get; set; }
    public Guid OrgLocationGUID { get; set; }
    public string HISNumber { get; set; } = "";
    public string Ward { get; set; } = "";
    public string BedNumber { get; set; } = "";
    public string Physician { get; set; } = "";
    public string AdmissionType { get; set; } = "Inpatient";
    public string? Diagnosis { get; set; }
    public string? Insurance { get; set; }
    public string? BloodGroup { get; set; }
    public string TransfusionHistory { get; set; } = "Unknown";
    public bool Allergy { get; set; }
    public bool Leukoreduced { get; set; }
    public bool Irradiated { get; set; }
    public bool Alloantibody { get; set; }
    public string? Notes { get; set; }
    public DateTime AdmittedAt { get; set; }
    public string IdentityStatus { get; set; } = "Pending";
    public DateTime? IdentityReviewedAt { get; set; }
    public string WristbandCode { get; set; } = "";
    public string Status { get; set; } = "Active";
    public DateTime? HistoryReviewedAt { get; set; }
    public int ClearanceChecklist { get; set; }
    public string? ClearanceNotes { get; set; }
    public DateTime? ClearanceReviewedAt { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual Patient Patient { get; set; } = null!;
    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual ICollection<PatientAdmissionReview> Reviews { get; set; } = new List<PatientAdmissionReview>();
}
