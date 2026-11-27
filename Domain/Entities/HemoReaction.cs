using Domain.Attributes;
namespace Domain.Entities;
[ClassName("پرونده واکنش نامطلوب")]
public class HemoReaction : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public Guid BloodTransferGUID { get; set; }
    public Guid ClientReportId { get; set; }
    public string CaseNumber { get; set; } = "";
    public string Status { get; set; } = "Reported";
    public DateTime OnsetAt { get; set; }
    public DateTime ReportedAt { get; set; }
    public Guid ReporterUserGUID { get; set; }
    public string SymptomsJson { get; set; } = "[]";
    public string InitialSeverity { get; set; } = "";
    public string InitialNotes { get; set; } = "";
    public string ActionsTaken { get; set; } = "";
    public string? SamplesSent { get; set; }
    public decimal? VolumeMl { get; set; }
    public Guid? DoctorUserGUID { get; set; }
    public DateTime? DoctorReviewedAt { get; set; }
    public string? DiagnosisCode { get; set; }
    public string? Severity { get; set; }
    public string? Imputability { get; set; }
    public string? Outcome { get; set; }
    public string? DoctorNotes { get; set; }
    public string? DoctorActions { get; set; }
    public string? DoctorLabRequest { get; set; }
    public string? FutureRecommendations { get; set; }
    public string? DoctorReference { get; set; }
    public Guid? BankUserGUID { get; set; }
    public DateTime? BankReviewedAt { get; set; }
    public string? BankFindings { get; set; }
    public string? BankSampleReference { get; set; }
    public string? BankReportReference { get; set; }
    public string? BankDisposition { get; set; }
    public string? InvestigationNotes { get; set; }
    public string ExternalStatus { get; set; } = "NotRecorded";
    public string? ExternalReference { get; set; }
    public bool? RcaRequired { get; set; }
    public string? RcaLead { get; set; }
    public string? RcaMethod { get; set; }
    public DateTime? RcaMeetingAt { get; set; }
    public string? RcaParticipants { get; set; }
    public string? DirectCause { get; set; }
    public string? RootCause { get; set; }
    public DateTime? RcaPublishedAt { get; set; }
    public Guid? RcaPublishedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual BloodTransfer Transfer { get; set; } = null!;
}
