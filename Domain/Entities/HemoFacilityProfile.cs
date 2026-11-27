using Domain.Attributes;
namespace Domain.Entities;
[ClassName("اطلاعات مرکز هموویژیلانس")]
public class HemoFacilityProfile : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public string FacilityName { get; set; } = "";
    public string? AccreditationCode { get; set; }
    public string? Chair { get; set; }
    public string? Secretary { get; set; }
    public string? Officer { get; set; }
    public string? BloodCenter { get; set; }
    public string? BloodCenterCode { get; set; }
    public string? ReportFooter { get; set; }
    public byte[] Version { get; set; } = Array.Empty<byte>();
    public virtual OrgLocation OrgLocation { get; set; } = null!;
}
