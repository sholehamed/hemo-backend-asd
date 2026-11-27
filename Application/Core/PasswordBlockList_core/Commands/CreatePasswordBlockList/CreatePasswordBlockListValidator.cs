
using Application.Common.Utility.Validator;
namespace Application.Core.PasswordBlockList_core.Commands.CreatePasswordBlockList;

public class CreatePasswordBlockListValidator : AbstractValidator<CreatePasswordBlockListCommand>
{
    public CreatePasswordBlockListValidator()
    {
       RuleFor(v => v.Password).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("پسورد",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پسورد"));
       

    }
}

