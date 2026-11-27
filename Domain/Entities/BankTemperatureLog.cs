using Domain.Attributes;
namespace Domain.Entities;
[ClassName("ثبت پایش دما")]
public class BankTemperatureLog : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public string EquipmentCode { get; set; } = "";
    public DateTime MeasuredAt { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal Temperature { get; set; }
    public string Assessment { get; set; } = "";
    public string? Notes { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public virtual OrgLocation OrgLocation { get; set; } = null!;
}
