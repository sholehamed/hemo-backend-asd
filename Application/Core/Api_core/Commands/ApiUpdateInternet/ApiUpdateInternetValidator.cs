
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Api_core.Commands.ApiUpdateInternet;

public class ApiUpdateInternetValidator : AbstractValidator<ApiUpdateInternetCommand>
{
    public ApiUpdateInternetValidator()
    {
       RuleFor(v => v.Internet).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("قابل نمایش در اینترنت"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

