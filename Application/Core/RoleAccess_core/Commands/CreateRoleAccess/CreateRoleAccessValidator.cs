
using Application.Common.Utility.Validator;
namespace Application.Core.RoleAccess_core.Commands.CreateRoleAccess;

public class CreateRoleAccessValidator : AbstractValidator<CreateRoleAccessCommand>
{
    public CreateRoleAccessValidator()
    {
       RuleFor(v => v.MenuGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("منو"));
       RuleFor(v => v.RoleGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نقش"));
       RuleFor(v => v.ApiGUID);

    }
}

