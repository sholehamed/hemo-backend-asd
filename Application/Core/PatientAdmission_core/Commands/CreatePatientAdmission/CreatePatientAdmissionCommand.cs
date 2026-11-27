using Application.Common.Exceptions;
using Application.Common.Utility.Security;

namespace Application.Core.PatientAdmission_core.Commands.CreatePatientAdmission;

public record CreatePatientAdmissionCommand : IRequest<Guid>
{
    public string IdentificationType { get; init; } = "NationalId";
    public string IdentificationNumber { get; init; } = "";
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string? FatherName { get; init; }
    public DateTime? BirthDate { get; init; }
    public string Gender { get; init; } = "Unknown";
    public string? Mobile { get; init; }
    public string HisNumber { get; init; } = "";
    public string Ward { get; init; } = "";
    public string BedNumber { get; init; } = "";
    public string Physician { get; init; } = "";
    public string AdmissionType { get; init; } = "Inpatient";
    public string? Diagnosis { get; init; }
    public string? Insurance { get; init; }
    public string? BloodGroup { get; init; }
    public string TransfusionHistory { get; init; } = "Unknown";
    public bool Allergy { get; init; }
    public bool Leukoreduced { get; init; }
    public bool Irradiated { get; init; }
    public bool Alloantibody { get; init; }
    public string? Notes { get; init; }
}

public class CreatePatientAdmissionCommandHandler(PatientService patients, PatientAdmissionService admissions, UserRoleService roles, AuthService auth) : IRequestHandler<CreatePatientAdmissionCommand, Guid>
{
    public async Task<Guid> Handle(CreatePatientAdmissionCommand r, CancellationToken ct)
    {
        var scope = await AdmissionScope.GetAsync(roles,auth,"post","/Identity/PatientAdmission",ct);
        var identity = r.IdentificationType == "Unknown" ? "UNK-" + Guid.NewGuid().ToString("N").ToUpperInvariant() : AdmissionRules.NormalizeIdentity(r.IdentificationNumber);
        var number = AdmissionRules.NormalizeIdentity(r.HisNumber);
        if (await admissions.Table.AnyAsync(x=>x.OrgLocationGUID==scope.OrgLocationGUID && x.HISNumber==number,ct))
            throw new ServiceExeption("شماره پرونده در این مرکز قبلاً ثبت شده است.");
        var patient = await patients.Table.SingleOrDefaultAsync(x=>x.OrgLocationGUID==scope.OrgLocationGUID && x.IdentificationType==r.IdentificationType && x.IdentificationNumber==identity,ct);
        var firstName = string.IsNullOrWhiteSpace(r.FirstName) ? "مجهول" : r.FirstName.Trim();
        var lastName = string.IsNullOrWhiteSpace(r.LastName) ? "الهویه" : r.LastName.Trim();
        if(patient == null)
        {
            patient = new Patient { OrgLocationGUID=scope.OrgLocationGUID, IdentificationType=r.IdentificationType, IdentificationNumber=identity, FirstName=firstName, LastName=lastName, FatherName=r.FatherName, BirthDate=r.BirthDate, Gender=r.Gender, Mobile=r.Mobile };
            patients.Table.Add(patient);
        }
        else if(patient.FirstName!=firstName || patient.LastName!=lastName || patient.BirthDate?.Date!=r.BirthDate?.Date)
            throw new ServiceExeption("مشخصات با پرونده موجود یکسان نیست؛ ابتدا پرونده را بررسی کنید.");
        var a = new PatientAdmission { PatientGUID=patient.GUID, OrgLocationGUID=scope.OrgLocationGUID, HISNumber=number, Ward=r.Ward.Trim(), BedNumber=r.BedNumber.Trim(), Physician=r.Physician.Trim(), AdmissionType=r.AdmissionType, Diagnosis=r.Diagnosis, Insurance=r.Insurance, BloodGroup=string.IsNullOrWhiteSpace(r.BloodGroup)?null:r.BloodGroup, TransfusionHistory=r.TransfusionHistory, Allergy=r.Allergy, Leukoreduced=r.Leukoreduced, Irradiated=r.Irradiated, Alloantibody=r.Alloantibody, Notes=r.Notes, AdmittedAt=DateTime.UtcNow, WristbandCode="HEMO-"+Guid.NewGuid().ToString("N").ToUpperInvariant() };
        admissions.Table.Add(a);
        try { await admissions.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new ServiceExeption("ثبت پرونده انجام نشد؛ احتمال ثبت همزمان یا تکراری وجود دارد. دوباره بررسی کنید."); }
        return a.GUID;
    }
}
