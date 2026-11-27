using Application.Common.Utility.Validator;

namespace Application.Core.sso_core.Commands.ssoSsoLogin;

public class ssoSsoLoginValidator : AbstractValidator<ssoSsoLoginCommand>
{
    public ssoSsoLoginValidator()
    {
        RuleFor(v => v.Key).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("کلید", 256))
            .NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
    }
}