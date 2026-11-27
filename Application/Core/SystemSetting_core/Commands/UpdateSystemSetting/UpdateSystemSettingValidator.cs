
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.SystemSetting_core.Commands.UpdateSystemSetting;

public class UpdateSystemSettingValidator : AbstractValidator<UpdateSystemSettingCommand>
{
    public UpdateSystemSettingValidator()
    {
       RuleFor(v => v.TokenExpireSeconds).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("اعتبار توکن", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار توکن"));
       RuleFor(v => v.RefreshTokenExpireSeconds).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("اعتبار رفرش توکن", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار رفرش توکن"));
       RuleFor(v => v.LoginAttempts).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("تعداد تلاش ناموق جهت ورود", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("تعداد تلاش ناموق جهت ورود"));
       RuleFor(v => v.LockUserSeconds).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("مدت زمان مسدود سازی کاربر به دلیل تلاش ناموفق برای ورود", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("مدت زمان مسدود سازی کاربر به دلیل تلاش ناموفق برای ورود"));
       RuleFor(v => v.LoginApiGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار توکن"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
       

    }
}

