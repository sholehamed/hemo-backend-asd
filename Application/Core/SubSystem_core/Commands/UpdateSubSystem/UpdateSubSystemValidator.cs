
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.SubSystem_core.Commands.UpdateSubSystem;

public class UpdateSubSystemValidator : AbstractValidator<UpdateSubSystemCommand>
{
    public UpdateSubSystemValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.Host).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس سرویس",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("آدرس سرویس"));
       RuleFor(v => v.Port).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("پورت سرویس", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پورت سرویس"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

