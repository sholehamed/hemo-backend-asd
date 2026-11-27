
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.SystemSetting_core.Commands.SystemSettingUpdateTokenInfo;

public class SystemSettingUpdateTokenInfoValidator : AbstractValidator<SystemSettingUpdateTokenInfoCommand>
{
    public SystemSettingUpdateTokenInfoValidator()
    {
       RuleFor(v => v.TokenExpireSeconds).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("اعتبار توکن", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار توکن"));
       RuleFor(v => v.RefreshTokenExpireSeconds).GreaterThan(0).WithMessage(ValidatorMassege.GetLessThanErrorMessage("اعتبار رفرش توکن", 0)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("اعتبار رفرش توکن"));
     
    }
}

