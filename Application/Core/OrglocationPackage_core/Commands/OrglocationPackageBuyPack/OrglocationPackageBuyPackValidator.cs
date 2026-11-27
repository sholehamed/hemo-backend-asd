
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.OrglocationPackage_core.Commands.OrglocationPackageBuyPack;

public class OrglocationPackageBuyPackValidator : AbstractValidator<OrglocationPackageBuyPackCommand>
{
    public OrglocationPackageBuyPackValidator()
    {
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
    }
}

