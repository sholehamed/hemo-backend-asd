
using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.UserNotificationLogin;

public class UserNotificationLoginValidator : AbstractValidator<UserNotificationLoginCommand>
{
    public UserNotificationLoginValidator()
    {
       RuleFor(v => v.Username).MaximumLength(64).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام کاربری",64));
       RuleFor(v => v.Password).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("رمز عبور",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("رمز عبور"));
      
    }
}

