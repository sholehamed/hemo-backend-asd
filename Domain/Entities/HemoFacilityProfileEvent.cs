using Domain.Attributes;
namespace Domain.Entities;
[ClassName("تاریخچه اطلاعات مرکز")]
public class HemoFacilityProfileEvent : AuditedEntity
{
    public Guid HemoFacilityProfileGUID { get; set; }
    public Guid ActorUserGUID { get; set; }
    public Guid ActorRoleGUID { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Notes { get; set; } = "";
    public string? PreviousJson { get; set; }
    public string CurrentJson { get; set; } = "";
    public virtual HemoFacilityProfile Profile { get; set; } = null!;
}
