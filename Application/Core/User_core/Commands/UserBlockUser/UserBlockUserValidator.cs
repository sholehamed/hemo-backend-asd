
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.UserBlockUser;

public class UserBlockUserValidator : AbstractValidator<UserBlockUserCommand>
{
    public UserBlockUserValidator()
    {
      
       RuleFor(v => v.Blocked).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("حساب کاربری مسدود شده است"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
    }
}

