
using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.UserSignUp;

public class UserSignUpValidator : AbstractValidator<UserSignUpCommand>
{
    public UserSignUpValidator()
    {
       RuleFor(v => v.Username).MaximumLength(64).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام کاربری",64));
       RuleFor(v => v.Password).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("رمز عبور",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("رمز عبور"));
       RuleFor(v => v.Mobile).MaximumLength(16).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("موبایل",16));
       RuleFor(v => v.Email).MaximumLength(64).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("ایمیل",64));

    }
}

