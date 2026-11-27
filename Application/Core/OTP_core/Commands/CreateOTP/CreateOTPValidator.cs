
using Application.Common.Utility.Validator;
namespace Application.Core.OTP_core.Commands.CreateOTP;

public class CreateOTPValidator : AbstractValidator<CreateOTPCommand>
{
    public CreateOTPValidator()
    {
       RuleFor(v => v.Username).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام کاربری",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نام کاربری"));
       RuleFor(v => v.Code).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("کد یکبار مصرف",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کد یکبار مصرف"));
       RuleFor(v => v.ExDate).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("تاریخ انقضا"));
       RuleFor(v => v.TryCount).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("تعداد تلاش مجاز", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("تعداد تلاش مجاز"));
       RuleFor(v => v.Used).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("استفاده شده"));
       RuleFor(v => v.ConvertID).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("کد کانورت دیتا",256));

    }
}

