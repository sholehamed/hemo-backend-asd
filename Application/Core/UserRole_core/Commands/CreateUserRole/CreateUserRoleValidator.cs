
using Application.Common.Utility.Validator;
namespace Application.Core.UserRole_core.Commands.CreateUserRole;

public class CreateUserRoleValidator : AbstractValidator<CreateUserRoleCommand>
{
    public CreateUserRoleValidator()
    {
        RuleFor(v => v.UserGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کاربر"));
        RuleFor(v => v.RoleGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نقش"));
        RuleFor(v => v.ExpireDate).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("تاریخ انقضا"));
    }
}

