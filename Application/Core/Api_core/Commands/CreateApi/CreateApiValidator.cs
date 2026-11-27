
using Application.Common.Utility.Validator;
namespace Application.Core.Api_core.Commands.CreateApi;

public class CreateApiValidator : AbstractValidator<CreateApiCommand>
{
    public CreateApiValidator()
    {
       RuleFor(v => v.DownstreamPathTemplate).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("مسیر درخواست ارسالی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("مسیر درخواست ارسالی"));
       RuleFor(v => v.UpstreamPathTemplate).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("مسیر درخواست دریافتی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("مسیر درخواست دریافتی"));
       RuleFor(v => v.IsPublic).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("سرویس از نوع عمومی می باشد"));
       RuleFor(v => v.Priority).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("اولویت", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اولویت"));
       RuleFor(v => v.SubSystemGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("زیر سیستم"));
       RuleFor(v => v.APITypeName).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("متد درخواست",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("متد درخواست"));
       RuleFor(v => v.Active).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("فعال"));
       RuleFor(v => v.Title).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));

    }
}

