
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.RoleAccess_core.Commands.UpdateRoleAccess;

public class UpdateRoleAccessValidator : AbstractValidator<UpdateRoleAccessCommand>
{
    public UpdateRoleAccessValidator()
    {
       RuleFor(v => v.MenuGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("منو"));
       RuleFor(v => v.RoleGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نقش"));
       RuleFor(v => v.ApiGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("سرویس"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

