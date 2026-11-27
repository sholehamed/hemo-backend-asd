using Domain.Attributes;
namespace Domain.Entities;
[ClassName("شناسنامه تجهیز")]
public class HemoEquipment : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string BrandModel { get; set; } = "";
    public string Ward { get; set; } = "";
    public string? SerialNumber { get; set; }
    public string Status { get; set; } = "Hold";
    public DateTime? CalibrationAt { get; set; }
    public DateTime? CalibrationDueAt { get; set; }
    public DateTime? MaintenanceDueAt { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual OrgLocation OrgLocation { get; set; } = null!;
}
