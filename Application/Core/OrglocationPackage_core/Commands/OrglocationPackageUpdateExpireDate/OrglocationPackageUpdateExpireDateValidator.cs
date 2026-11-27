
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.OrglocationPackage_core.Commands.OrglocationPackageUpdateExpireDate;

public class OrglocationPackageUpdateExpireDateValidator : AbstractValidator<OrglocationPackageUpdateExpireDateCommand>
{
    public OrglocationPackageUpdateExpireDateValidator()
    {
       RuleFor(v => v.ExpiryDate).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

