
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.UserInvite_core.Commands.UpdateUserInvite;

public class UpdateUserInviteValidator : AbstractValidator<UpdateUserInviteCommand>
{
    public UpdateUserInviteValidator()
    {
       RuleFor(v => v.UserGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کاربر"));
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));
       RuleFor(v => v.UserAccepted);
       RuleFor(v => v.OrgLocationAccepted);
       RuleFor(v => v.UserInfo).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("کاربر",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کاربر"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

