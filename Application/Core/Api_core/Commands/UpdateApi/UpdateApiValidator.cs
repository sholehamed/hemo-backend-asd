
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Api_core.Commands.UpdateApi;

public class UpdateApiValidator : AbstractValidator<UpdateApiCommand>
{
    public UpdateApiValidator()
    {
       RuleFor(v => v.IsPublic).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("سرویس از نوع عمومی می باشد"));
       RuleFor(v => v.Active).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("فعال"));
       RuleFor(v => v.Title).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

