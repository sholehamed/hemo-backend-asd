
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.UserRole_core.Commands.UpdateUserRole;

public class UpdateUserRoleValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleValidator()
    {
        RuleFor(v => v.UserGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کاربر"));
        RuleFor(v => v.RoleGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نقش"));
        RuleFor(v => v.ExpireDate).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("تاریخ انقضا"));
        RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

