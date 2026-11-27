
using Application.Common.Utility.Validator;
namespace Application.Core.TicketStatus_core.Commands.CreateTicketStatus;

public class CreateTicketStatusValidator : AbstractValidator<CreateTicketStatusCommand>
{
    public CreateTicketStatusValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.Gkey).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("کد", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کد"));

    }
}

