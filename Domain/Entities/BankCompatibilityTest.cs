using Domain.Attributes;
namespace Domain.Entities;
[ClassName("نتیجه ثبت‌شده آزمایش سازگاری")]
public class BankCompatibilityTest : AuditedEntity
{
    public Guid BankBloodUnitGUID { get; set; }
    public Guid BloodRequestGUID { get; set; }
    public int SubmissionAttempt { get; set; }
    public byte[]? RequestVersion { get; set; }
    public byte[]? AdmissionVersion { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public DateTime RecordedAt { get; set; }
    public string SampleReference { get; set; } = "";
    public string SecondSample { get; set; } = "";
    public string LabelGroup { get; set; } = "";
    public string Method { get; set; } = "";
    public string ImmediateSpin { get; set; } = "";
    public string Phase37 { get; set; } = "";
    public string Ahg { get; set; } = "";
    public string Dat { get; set; } = "";
    public string AntibodyScreen { get; set; } = "";
    public string ReportReference { get; set; } = "";
    public string Conclusion { get; set; } = "";
    public string? Notes { get; set; }
    public virtual BankBloodUnit Unit { get; set; } = null!;
    public virtual BloodRequest BloodRequest { get; set; } = null!;
}
