
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Api_core.Commands.ApiUpdateLogData;

public class ApiUpdateLogDataValidator : AbstractValidator<ApiUpdateLogDataCommand>
{
    public ApiUpdateLogDataValidator()
    {
       RuleFor(v => v.LogData).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("لاگ فعال باشد"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
    }
}

