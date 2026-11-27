using Domain.Attributes;
namespace Domain.Entities;
[ClassName("بیمار")]
public class Patient : AuditedEntity
{
    public Guid OrgLocationGUID { get; set; }
    public string IdentificationType { get; set; } = "NationalId";
    public string IdentificationNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? FatherName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string Gender { get; set; } = "Unknown";
    public string? Mobile { get; set; }
    public virtual OrgLocation OrgLocation { get; set; } = null!;
    public virtual ICollection<PatientAdmission> Admissions { get; set; } = new List<PatientAdmission>();
}
