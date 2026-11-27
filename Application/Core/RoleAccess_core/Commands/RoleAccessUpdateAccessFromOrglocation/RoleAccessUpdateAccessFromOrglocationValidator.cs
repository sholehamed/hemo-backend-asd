
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.RoleAccess_core.Commands.RoleAccessUpdateAccessFromOrglocation;

public class RoleAccessUpdateAccessFromOrglocationValidator : AbstractValidator<RoleAccessUpdateAccessFromOrglocationCommand>
{
    public RoleAccessUpdateAccessFromOrglocationValidator()
    {
       RuleFor(v => v.RoleGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نقش"));
       RuleFor(v => v.OrglocationPackageGUID);

    }
}

