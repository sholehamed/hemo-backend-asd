
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.UserInvite_core.Commands.UserInviteUpdateUserInvite;

public class UserInviteUpdateUserInviteValidator : AbstractValidator<UserInviteUpdateUserInviteCommand>
{
    public UserInviteUpdateUserInviteValidator()
    {
       RuleFor(v => v.UserAccepted);
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

