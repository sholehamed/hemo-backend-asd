
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.TicketStatus_core.Commands.UpdateTicketStatus;

public class UpdateTicketStatusValidator : AbstractValidator<UpdateTicketStatusCommand>
{
    public UpdateTicketStatusValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.Gkey).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("کد", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کد"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

