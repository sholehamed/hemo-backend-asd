using Domain.Attributes;
namespace Domain.Entities;
[ClassName("گردش واحد فرآورده")]
public class BankUnitEvent : AuditedEntity
{
    public Guid BankBloodUnitGUID { get; set; }
    public Guid? BloodRequestGUID { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public string Action { get; set; } = "";
    public string PreviousStatus { get; set; } = "";
    public string NewStatus { get; set; } = "";
    public DateTime OccurredAt { get; set; }
    public string? Reason { get; set; }
    public string? Method { get; set; }
    public decimal? Temperature { get; set; }
    public DateTime? EffectiveExpiry { get; set; }
    public virtual BankBloodUnit Unit { get; set; } = null!;
    public virtual BloodRequest? BloodRequest { get; set; }
}
