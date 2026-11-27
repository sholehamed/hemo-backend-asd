
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.BlockListIP_core.Commands.UpdateBlockListIP;

public class UpdateBlockListIPValidator : AbstractValidator<UpdateBlockListIPCommand>
{
    public UpdateBlockListIPValidator()
    {
       RuleFor(v => v.IPAddress).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس ای پی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("آدرس ای پی"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
       

    }
}

