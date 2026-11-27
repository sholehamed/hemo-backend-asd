
using Application.Common.Utility.Validator;
namespace Application.Core.SubSystem_core.Commands.CreateSubSystem;

public class CreateSubSystemValidator : AbstractValidator<CreateSubSystemCommand>
{
    public CreateSubSystemValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.Host).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس سرویس",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("آدرس سرویس"));
       RuleFor(v => v.Port).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("پورت سرویس", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پورت سرویس"));

    }
}

