
using Application.Common.Utility.Validator;
namespace Application.Core.MenuApi_core.Commands.CreateMenuApi;

public class CreateMenuApiValidator : AbstractValidator<CreateMenuApiCommand>
{
    public CreateMenuApiValidator()
    {
       RuleFor(v => v.MenuGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("منو"));

    }
}

