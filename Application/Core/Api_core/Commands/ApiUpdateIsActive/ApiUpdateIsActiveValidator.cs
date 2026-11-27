
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Api_core.Commands.ApiUpdateIsActive;

public class ApiUpdateIsActiveValidator : AbstractValidator<ApiUpdateIsActiveCommand>
{
    public ApiUpdateIsActiveValidator()
    {

       RuleFor(v => v.Active).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("فعال"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

