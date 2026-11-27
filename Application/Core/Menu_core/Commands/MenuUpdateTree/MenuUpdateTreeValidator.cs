
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Menu_core.Commands.MenuUpdateTree;

public class MenuUpdateTreeValidator : AbstractValidator<MenuUpdateTreeCommand>
{
    public MenuUpdateTreeValidator()
    {
       RuleFor(v => v.SourceGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید مبدا"));
       RuleFor(v => v.DestGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید مقصد"));

    }
}

