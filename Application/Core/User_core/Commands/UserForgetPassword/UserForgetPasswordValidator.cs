using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.UserForgetPassword;

public class UserForgetPasswordValidator : AbstractValidator<UserForgetPasswordCommand>
{
    public UserForgetPasswordValidator()
    {
        RuleFor(v => v.Username).MaximumLength(64).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام کاربری",64));
      
    }
}