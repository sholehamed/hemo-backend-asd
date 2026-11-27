
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.ShoppingCartProduct_core.Commands.UpdateShoppingCartProduct;

public class UpdateShoppingCartProductValidator : AbstractValidator<UpdateShoppingCartProductCommand>
{
    public UpdateShoppingCartProductValidator()
    {
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
       RuleFor(v => v.ShoppingCartGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("سبد خرید"));
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

