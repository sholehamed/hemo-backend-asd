
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Menu_core.Commands.UpdateMenu;

public class UpdateMenuValidator : AbstractValidator<UpdateMenuCommand>
{
    public UpdateMenuValidator()
    {
       RuleFor(v => v.label).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.icon).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آیکون",256));
       RuleFor(v => v.routerLink).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس",256));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

