
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.OrglocationPackage_core.Commands.OrglocationPackageUpdateVisibility;

public class OrglocationPackageUpdateVisibilityValidator : AbstractValidator<OrglocationPackageUpdateVisibilityCommand>
{
    public OrglocationPackageUpdateVisibilityValidator()
    {
       RuleFor(v => v.Hidden).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج پنهان باشد"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
    }
}

