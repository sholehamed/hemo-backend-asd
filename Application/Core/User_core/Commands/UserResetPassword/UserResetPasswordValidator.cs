using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;

namespace Application.Core.User_core.Commands.UserResetPassword;

public class UserResetPasswordValidator : AbstractValidator<UserResetPasswordCommand>
{
    public UserResetPasswordValidator()
    {
        RuleFor(v => v.Password).MaximumLength(256)
            .WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("رمز عبور", 256)).NotNull()
            .WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("رمز عبور"));
    }
}