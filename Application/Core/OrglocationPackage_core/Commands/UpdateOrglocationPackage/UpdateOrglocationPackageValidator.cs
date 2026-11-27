
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.OrglocationPackage_core.Commands.UpdateOrglocationPackage;

public class UpdateOrglocationPackageValidator : AbstractValidator<UpdateOrglocationPackageCommand>
{
    public UpdateOrglocationPackageValidator()
    {
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
       RuleFor(v => v.ExpiryDate).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

