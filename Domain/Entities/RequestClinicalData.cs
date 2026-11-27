using Domain.Attributes;
namespace Domain.Entities;
[ClassName("اطلاعات بالینی درخواست")]
public class RequestClinicalData : AuditedEntity
{
    public Guid BloodRequestGUID { get; set; }
    public string? Diagnosis { get; set; }
    public string? ClinicalIndication { get; set; }
    public decimal? Hemoglobin { get; set; }
    public decimal? Hematocrit { get; set; }
    public int? PlateletCount { get; set; }
    public bool? RecentTransfusion { get; set; }
    public bool? RecentPregnancy { get; set; }
    public bool? PriorReaction { get; set; }
    public bool? UnexpectedAntibody { get; set; }
    public string? Premedication { get; set; }
    public string? SampleStatus { get; set; }
    public string? Notes { get; set; }
    public virtual BloodRequest BloodRequest { get; set; } = null!;

}
