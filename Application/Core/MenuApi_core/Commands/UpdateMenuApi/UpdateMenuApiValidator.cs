
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.MenuApi_core.Commands.UpdateMenuApi;

public class UpdateMenuApiValidator : AbstractValidator<UpdateMenuApiCommand>
{
    public UpdateMenuApiValidator()
    {
       RuleFor(v => v.MenuGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("منو"));
       RuleFor(v => v.ApiUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("سرویس"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

