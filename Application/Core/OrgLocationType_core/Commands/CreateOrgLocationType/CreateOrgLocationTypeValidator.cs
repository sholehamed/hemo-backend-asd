
using Application.Common.Utility.Validator;
namespace Application.Core.OrgLocationType_core.Commands.CreateOrgLocationType;

public class CreateOrgLocationTypeValidator : AbstractValidator<CreateOrgLocationTypeCommand>
{
    public CreateOrgLocationTypeValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));

    }
}

