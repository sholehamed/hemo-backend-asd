
using Application.Common.Utility.Validator;
namespace Application.Core.BlockListIP_core.Commands.CreateBlockListIP;

public class CreateBlockListIPValidator : AbstractValidator<CreateBlockListIPCommand>
{
    public CreateBlockListIPValidator()
    {
       RuleFor(v => v.IPAddress).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس ای پی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("آدرس ای پی"));
       

    }
}

