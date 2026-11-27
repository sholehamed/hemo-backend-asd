
using Application.Common.Utility.Validator;
namespace Application.Core.Menu_core.Commands.CreateMenu;

public class CreateMenuValidator : AbstractValidator<CreateMenuCommand>
{
    public CreateMenuValidator()
    {
       RuleFor(v => v.PMenuGUID);
       RuleFor(v => v.label).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.icon).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آیکون",256));
       RuleFor(v => v.routerLink).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس",256));
       RuleFor(v => v.ShowInMenu).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("ترتیب نمایش"));

    }
}

