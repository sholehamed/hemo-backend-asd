
using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.UserLogin;

public class UserLoginValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginValidator()
    {
       RuleFor(v => v.Username).MaximumLength(64).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام کاربری",16)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نام کاربری"));
       RuleFor(v => v.Password).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("رمز عبور",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("رمز عبور"));

    }
}

