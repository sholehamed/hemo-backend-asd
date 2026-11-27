
using Application.Common.Utility.Validator;
namespace Application.Core.Department_core.Commands.CreateDepartment;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.Gkey).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("کد", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کد"));

    }
}

