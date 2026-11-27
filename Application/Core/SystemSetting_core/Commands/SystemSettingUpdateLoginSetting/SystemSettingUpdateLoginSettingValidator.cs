
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.SystemSetting_core.Commands.SystemSettingUpdateLoginSetting;

public class SystemSettingUpdateLoginSettingValidator : AbstractValidator<SystemSettingUpdateLoginSettingCommand>
{
    public SystemSettingUpdateLoginSettingValidator()
    {
       RuleFor(v => v.LoginAttempts).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("تعداد تلاش ناموق جهت ورود", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("تعداد تلاش ناموق جهت ورود"));
       RuleFor(v => v.LockUserSeconds).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("مدت زمان مسدود سازی کاربر به دلیل تلاش ناموفق برای ورود", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("مدت زمان مسدود سازی کاربر به دلیل تلاش ناموفق برای ورود"));

    }
}

