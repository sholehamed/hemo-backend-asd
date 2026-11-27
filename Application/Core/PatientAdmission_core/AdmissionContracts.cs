namespace Application.Core.PatientAdmission_core;

public record AdmissionView(Guid Guid, Guid PatientGuid, string IdentificationType, string IdentificationNumber, string FirstName, string LastName, string? FatherName, DateTime? BirthDate, string Gender, string? Mobile, string HisNumber, string Ward, string BedNumber, string Physician, string AdmissionType, string? Diagnosis, string? Insurance, string? BloodGroup, string TransfusionHistory, bool Allergy, bool Leukoreduced, bool Irradiated, bool Alloantibody, string? Notes, DateTime AdmittedAt, string IdentityStatus, DateTime? IdentityReviewedAt, string WristbandCode, string Status, DateTime? HistoryReviewedAt, int ClearanceChecklist, string? ClearanceNotes, DateTime? ClearanceReviewedAt, byte[] Version);
public record AdmissionReviewView(string Kind, string Outcome, int Checklist, string? Notes, DateTime ReviewedAt);
public record AdmissionStats(int Total, int Active, int Verified, int Mismatch, int Ready, List<AdmissionGroup> Wards, List<AdmissionGroup> BloodGroups);
public record AdmissionGroup(string Name, int Count);
public record PatientIdentityView(Guid Guid, string FirstName, string LastName, string? FatherName, DateTime? BirthDate, string Gender, string? Mobile);

internal static class AdmissionProjection
{
    public static readonly System.Linq.Expressions.Expression<Func<PatientAdmission, AdmissionView>> View = a => new AdmissionView(a.GUID,a.PatientGUID,a.Patient.IdentificationType,a.Patient.IdentificationNumber,a.Patient.FirstName,a.Patient.LastName,a.Patient.FatherName,a.Patient.BirthDate,a.Patient.Gender,a.Patient.Mobile,a.HISNumber,a.Ward,a.BedNumber,a.Physician,a.AdmissionType,a.Diagnosis,a.Insurance,a.BloodGroup,a.TransfusionHistory,a.Allergy,a.Leukoreduced,a.Irradiated,a.Alloantibody,a.Notes,a.AdmittedAt,a.IdentityStatus,a.IdentityReviewedAt,a.WristbandCode,a.Status,a.HistoryReviewedAt,a.ClearanceChecklist,a.ClearanceNotes,a.ClearanceReviewedAt,a.Version);
}

public static class AdmissionRules
{
    public static string NormalizeIdentity(string? value) => string.Concat((value ?? "").Trim().Where(c => !char.IsWhiteSpace(c)).Select(c => c >= '۰' && c <= '۹' ? (char)('0' + c - '۰') : c >= '٠' && c <= '٩' ? (char)('0' + c - '٠') : char.ToUpperInvariant(c)));
    public static bool IsNationalId(string? value)
    {
        var s = NormalizeIdentity(value);
        if (s.Length != 10 || s.Any(c => c < '0' || c > '9') || s.All(c => c == s[0])) return false;
        var rem = Enumerable.Range(0,9).Sum(i => (s[i]-'0')*(10-i)) % 11;
        return s[9]-'0' == (rem < 2 ? rem : 11-rem);
    }
    public static bool CompleteChecklist(int checklist) => checklist == 31;
}
