
using Application.Common.Utility.Validator;
namespace Application.Core.OrglocationPackage_core.Commands.CreateOrglocationPackage;

public class CreateOrglocationPackageValidator : AbstractValidator<CreateOrglocationPackageCommand>
{
    public CreateOrglocationPackageValidator()
    {
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
       RuleFor(v => v.ExpiryDate).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار"));

    }
}

