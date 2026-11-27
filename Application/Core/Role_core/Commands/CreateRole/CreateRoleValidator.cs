
using Application.Common.Utility.Validator;
namespace Application.Core.Role_core.Commands.CreateRole;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
       RuleFor(v => v.PRoleGUID);
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));

    }
}

