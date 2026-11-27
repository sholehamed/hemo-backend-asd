
using Application.Common.Utility.Validator;
namespace Application.Core.ShoppingCartProduct_core.Commands.CreateShoppingCartProduct;

public class CreateShoppingCartProductValidator : AbstractValidator<CreateShoppingCartProductCommand>
{
    public CreateShoppingCartProductValidator()
    {
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));

    }
}

