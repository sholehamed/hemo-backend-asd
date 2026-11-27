
using Application.Common.Utility.Validator;
namespace Application.Core.sso_core.Commands.Createsso;

public class CreatessoValidator : AbstractValidator<CreatessoCommand>
{
    public CreatessoValidator()
    {
       RuleFor(v => v.UserGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کاربر"));
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
       RuleFor(v => v.IPAddress).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("آدرس IP",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("آدرس IP"));
       RuleFor(v => v.UserAgent).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("مرورگر",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("مرورگر"));
       RuleFor(v => v.ExpireDate).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("تاریخ انقضا"));
       RuleFor(v => v.Key).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("کلید",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
       RuleFor(v => v.Sign).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("اعتبارسنجی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبارسنجی"));

    }
}

