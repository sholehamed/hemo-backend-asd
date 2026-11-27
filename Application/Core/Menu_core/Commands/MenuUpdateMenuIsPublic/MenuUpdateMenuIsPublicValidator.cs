
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Menu_core.Commands.MenuUpdateMenuIsPublic;

public class MenuUpdateMenuIsPublicValidator : AbstractValidator<MenuUpdateMenuIsPublicCommand>
{
    public MenuUpdateMenuIsPublicValidator()
    {
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

