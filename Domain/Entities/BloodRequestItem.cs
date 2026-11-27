using Domain.Attributes;
namespace Domain.Entities;
[ClassName("قلم درخواست فرآورده")]
public class BloodRequestItem : AuditedEntity
{
    public Guid BloodRequestGUID { get; set; }
    public string ProductCode { get; set; } = "";
    public int Quantity { get; set; }
    public string? RequestedBloodGroup { get; set; }
    public string? SpecialInstructions { get; set; }
    public virtual BloodRequest BloodRequest { get; set; } = null!;

}
