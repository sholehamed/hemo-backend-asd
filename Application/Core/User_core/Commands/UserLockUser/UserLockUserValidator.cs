
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.UserLockUser;

public class UserLockUserValidator : AbstractValidator<UserLockUserCommand>
{
    public UserLockUserValidator()
    {

       RuleFor(v => v.Locked).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("حساب کاربری قفل شده است"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

