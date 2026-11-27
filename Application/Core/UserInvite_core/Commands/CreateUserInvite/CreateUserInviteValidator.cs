
using Application.Common.Utility.Validator;
namespace Application.Core.UserInvite_core.Commands.CreateUserInvite;

public class CreateUserInviteValidator : AbstractValidator<CreateUserInviteCommand>
{
    public CreateUserInviteValidator()
    {
       RuleFor(v => v.UserInfo).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("کاربر",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کاربر"));

    }
}

