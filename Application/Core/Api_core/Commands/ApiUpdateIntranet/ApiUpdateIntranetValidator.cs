
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Api_core.Commands.ApiUpdateIntranet;

public class ApiUpdateIntranetValidator : AbstractValidator<ApiUpdateIntranetCommand>
{
    public ApiUpdateIntranetValidator()
    {
       RuleFor(v => v.Intranet).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("قابل نمایش در اینترانت"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
    }
}

