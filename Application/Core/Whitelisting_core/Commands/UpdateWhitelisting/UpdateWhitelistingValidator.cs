
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Whitelisting_core.Commands.UpdateWhitelisting;

public class UpdateWhitelistingValidator : AbstractValidator<UpdateWhitelistingCommand>
{
    public UpdateWhitelistingValidator()
    {
       RuleFor(v => v.IPAddress).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس ای پی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("آدرس ای پی"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
       

    }
}

