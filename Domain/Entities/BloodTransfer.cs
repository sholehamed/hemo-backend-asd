using Domain.Attributes;
namespace Domain.Entities;
[ClassName("حواله و گردش تزریق واحد خون")]
public class BloodTransfer : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public Guid BankBloodUnitGUID { get; set; }
    public Guid BloodRequestGUID { get; set; }
    public Guid PatientAdmissionGUID { get; set; }
    public Guid BankCompatibilityTestGUID { get; set; }
    public string TransferNumber { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string HisNumber { get; set; } = "";
    public string Ward { get; set; } = "";
    public string WristbandCode { get; set; } = "";
    public string UnitCode { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string BloodGroup { get; set; } = "";
    public byte[]? RequestVersion { get; set; }
    public byte[]? AdmissionVersion { get; set; }
    public string Status { get; set; } = "Dispatched";
    public DateTime DispatchedAt { get; set; }
    public Guid DispatchedBy { get; set; }
    public string CarrierName { get; set; } = "";
    public string CarrierCode { get; set; } = "";
    public string ContainerCode { get; set; } = "";
    public decimal DispatchTemperature { get; set; }
    public string TransportIndicator { get; set; } = "";
    public int? PackCount { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public Guid? ReceivedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTime? WitnessedAt { get; set; }
    public Guid? WitnessedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public Guid? StartedBy { get; set; }
    public DateTime? EndedAt { get; set; }
    public Guid? EndedBy { get; set; }
    public decimal? VolumeMl { get; set; }
    public bool SuspectedReaction { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? ReturnAcceptedAt { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual BankBloodUnit Unit { get; set; } = null!;
    public virtual BloodRequest BloodRequest { get; set; } = null!;
    public virtual PatientAdmission Admission { get; set; } = null!;
    public virtual BankCompatibilityTest CompatibilityTest { get; set; } = null!;
}
