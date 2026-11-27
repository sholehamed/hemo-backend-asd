
using Application.Common.Utility.Validator;
namespace Application.Core.Whitelisting_core.Commands.CreateWhitelisting;

public class CreateWhitelistingValidator : AbstractValidator<CreateWhitelistingCommand>
{
    public CreateWhitelistingValidator()
    {
       RuleFor(v => v.IPAddress).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس ای پی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("آدرس ای پی"));
       

    }
}

