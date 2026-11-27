namespace Application.Core.PatientAdmission_core.Commands.CreatePatientAdmission;

public class CreatePatientAdmissionValidator : AbstractValidator<CreatePatientAdmissionCommand>
{
    public CreatePatientAdmissionValidator()
    {
        RuleFor(x=>x.IdentificationType).Must(x=>new[]{"NationalId","Passport","Unknown"}.Contains(x)).WithMessage("نوع شناسه نامعتبر است.");
        RuleFor(x=>x.IdentificationNumber).Must(AdmissionRules.IsNationalId).When(x=>x.IdentificationType=="NationalId").WithMessage("کد ملی معتبر نیست.");
        RuleFor(x=>x.IdentificationNumber).NotEmpty().MaximumLength(64).When(x=>x.IdentificationType=="Passport");
        RuleFor(x=>x.FirstName).MaximumLength(100); RuleFor(x=>x.LastName).MaximumLength(100);
        When(x=>x.IdentificationType!="Unknown",()=>{RuleFor(x=>x.FirstName).NotEmpty(); RuleFor(x=>x.LastName).NotEmpty(); RuleFor(x=>x.BirthDate).NotNull();});
        RuleFor(x=>x.BirthDate).Must(x=>!x.HasValue || (x.Value.Date<=DateTime.UtcNow.Date && x.Value.Year>=1900));
        RuleFor(x=>x.FatherName).MaximumLength(100); RuleFor(x=>x.Mobile).MaximumLength(32);
        RuleFor(x=>x.Gender).Must(x=>new[]{"Female","Male","Unknown"}.Contains(x));
        RuleFor(x=>x.HisNumber).NotEmpty().MaximumLength(64); RuleFor(x=>x.Ward).NotEmpty().MaximumLength(100); RuleFor(x=>x.BedNumber).NotEmpty().MaximumLength(64); RuleFor(x=>x.Physician).NotEmpty().MaximumLength(100);
        RuleFor(x=>x.AdmissionType).Must(x=>new[]{"Inpatient","Emergency","Outpatient"}.Contains(x));
        RuleFor(x=>x.Diagnosis).MaximumLength(500); RuleFor(x=>x.Insurance).MaximumLength(100);
        RuleFor(x=>x.BloodGroup).Must(x=>string.IsNullOrEmpty(x)||new[]{"A+","A-","B+","B-","AB+","AB-","O+","O-"}.Contains(x));
        RuleFor(x=>x.TransfusionHistory).Must(x=>new[]{"Unknown","None","Previous","Reaction"}.Contains(x));
        RuleFor(x=>x.Notes).MaximumLength(2000);
    }
}
