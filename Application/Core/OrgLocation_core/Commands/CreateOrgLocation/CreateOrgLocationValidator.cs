
using Application.Common.Utility.Validator;
namespace Application.Core.OrgLocation_core.Commands.CreateOrgLocation;

public class CreateOrgLocationValidator : AbstractValidator<CreateOrgLocationCommand>
{
    public CreateOrgLocationValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.POrgLocationGUID);
       RuleFor(v => v.Address).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس",256));
       RuleFor(v => v.Tell).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("تلفن",256));
       RuleFor(v => v.Fax).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("فکس",256));
       RuleFor(v => v.Email).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("ایمیل",256));
       RuleFor(v => v.Url).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس وبسایت",256));

    }
}

