using Application.Common.Exceptions;
using Application.Common.Utility.Security;

namespace Application.Core.PatientAdmission_core.Commands.UpdatePatientIdentity;

// Keep the patient's GUID and admissions; resolving a temporary identity never silently merges two records.
public record UpdatePatientIdentityCommand(Guid Guid,string IdentificationType,string IdentificationNumber,string FirstName,string LastName,string? FatherName,DateTime? BirthDate,string Gender,string? Mobile,string Reason,byte[] Version) : IRequest;
public class UpdatePatientIdentityValidator : AbstractValidator<UpdatePatientIdentityCommand>
{
    public UpdatePatientIdentityValidator()
    {
        RuleFor(x=>x.Guid).NotEmpty();RuleFor(x=>x.Version).NotEmpty();RuleFor(x=>x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x=>x.IdentificationType).Must(x=>new[]{"NationalId","Passport","Unknown"}.Contains(x));
        RuleFor(x=>x.IdentificationNumber).Must(AdmissionRules.IsNationalId).When(x=>x.IdentificationType=="NationalId");
        RuleFor(x=>x.IdentificationNumber).NotEmpty().MaximumLength(64).When(x=>x.IdentificationType=="Passport");
        RuleFor(x=>x.FirstName).NotEmpty().MaximumLength(100);RuleFor(x=>x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.BirthDate).NotNull().When(x=>x.IdentificationType!="Unknown");
        RuleFor(x=>x.BirthDate).Must(x=>!x.HasValue||(x.Value.Year>=1900&&x.Value.Date<=DateTime.UtcNow.Date));
        RuleFor(x=>x.FatherName).MaximumLength(100);RuleFor(x=>x.Mobile).MaximumLength(32);
        RuleFor(x=>x.Gender).Must(x=>new[]{"Female","Male","Unknown"}.Contains(x));
    }
}
public class UpdatePatientIdentityCommandHandler(PatientAdmissionService admissions,PatientService patients,PatientAdmissionReviewService reviews,UserRoleService roles,AuthService auth) : IRequestHandler<UpdatePatientIdentityCommand>
{
    public async Task Handle(UpdatePatientIdentityCommand r,CancellationToken ct)
    {
        var scope=await AdmissionScope.GetAsync(roles,auth,"put","/Identity/PatientAdmission/PatientIdentity",ct);
        var a=await admissions.Table.Include(x=>x.Patient).SingleOrDefaultAsync(x=>x.GUID==r.Guid&&x.OrgLocationGUID==scope.OrgLocationGUID,ct)??throw new ServiceExeption("پرونده در محدوده مجاز یافت نشد.");
        if(!a.Version.SequenceEqual(r.Version))throw new ServiceExeption("پرونده تغییر کرده است؛ دوباره بارگذاری کنید.");
        var p=a.Patient;
        var number=r.IdentificationType=="Unknown"?p.IdentificationNumber:AdmissionRules.NormalizeIdentity(r.IdentificationNumber);
        if(p.IdentificationType!="Unknown"&&(p.IdentificationType!=r.IdentificationType||p.IdentificationNumber!=number))
            throw new ServiceExeption("تغییر شناسه قطعی بیمار به بررسی اختصاصی و رفع پرونده تکراری نیاز دارد.");
        if(await patients.Table.AnyAsync(x=>x.OrgLocationGUID==scope.OrgLocationGUID&&x.GUID!=p.GUID&&x.IdentificationType==r.IdentificationType&&x.IdentificationNumber==number,ct))
            throw new ServiceExeption("این شناسه قبلاً ثبت شده است؛ ادغام پرونده به بررسی مسئول مجاز نیاز دارد.");
        var previous=p.IdentificationType;
        p.IdentificationType=r.IdentificationType;p.IdentificationNumber=number;p.FirstName=r.FirstName.Trim();p.LastName=r.LastName.Trim();p.FatherName=r.FatherName;p.BirthDate=r.BirthDate;p.Gender=r.Gender;p.Mobile=r.Mobile;
        var affected=await admissions.Table.Where(x=>x.PatientGUID==p.GUID&&x.OrgLocationGUID==scope.OrgLocationGUID).ToListAsync(ct);
        foreach(var admission in affected)
        {
            admission.IdentityStatus="Pending";admission.IdentityReviewedAt=null;admission.HistoryReviewedAt=null;
            admission.Status="Active";admission.ClearanceChecklist=0;admission.ClearanceNotes=null;admission.ClearanceReviewedAt=null;
            // Update every related admission in one transaction, including its row version.
            admission.LastModified=DateTime.UtcNow;
            reviews.Table.Add(new PatientAdmissionReview{PatientAdmissionGUID=admission.GUID,Kind="IdentityUpdate",Outcome=previous=="Unknown"&&r.IdentificationType!="Unknown"?"Resolved":"Updated",Checklist=0,Notes=r.Reason,ReviewedAt=DateTime.UtcNow});
        }
        try{await admissions.SaveChangesAsync(ct);}
        catch(DbUpdateConcurrencyException){throw new ServiceExeption("پرونده همزمان تغییر کرده است؛ دوباره بارگذاری کنید.");}
        catch(DbUpdateException){throw new ServiceExeption("اصلاح مشخصات ثبت نشد؛ شناسه و پرونده را دوباره بررسی کنید.");}
    }
}
