
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Menu_core.Commands.MenuUpdateVisibility;

public class MenuUpdateVisibilityValidator : AbstractValidator<MenuUpdateVisibilityCommand>
{
    public MenuUpdateVisibilityValidator()
    {

       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

