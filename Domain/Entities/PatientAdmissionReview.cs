using Domain.Attributes;
namespace Domain.Entities;
[ClassName("بررسی پذیرش")]
public class PatientAdmissionReview : AuditedEntity
{
    public Guid PatientAdmissionGUID { get; set; }
    public string Kind { get; set; } = "";
    public string Outcome { get; set; } = "";
    public int Checklist { get; set; }
    public string? Notes { get; set; }
    public DateTime ReviewedAt { get; set; }
    public virtual PatientAdmission PatientAdmission { get; set; } = null!;

}
