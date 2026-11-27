using Domain.Attributes;
namespace Domain.Entities;
[ClassName("واحد فرآورده بانک خون")]
public class BankBloodUnit : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public string UnitCode { get; set; } = "";
    public string ProductCode { get; set; } = "";
    public string BloodGroup { get; set; } = "";
    public string? BatchNumber { get; set; }
    public string SupplySource { get; set; } = "";
    public string EquipmentCode { get; set; } = "";
    public DateTime ReceivedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Status { get; set; } = "ReceivedHold";
    public DateTime? InspectionAt { get; set; }
    public Guid? BloodRequestGUID { get; set; }
    public DateTime? ThawStartedAt { get; set; }
    public DateTime? ThawCompletedAt { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual BloodRequest? BloodRequest { get; set; }
}
