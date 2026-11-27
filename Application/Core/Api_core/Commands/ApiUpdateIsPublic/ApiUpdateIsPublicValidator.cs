
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.Api_core.Commands.ApiUpdateIsPublic;

public class ApiUpdateIsPublicValidator : AbstractValidator<ApiUpdateIsPublicCommand>
{
    public ApiUpdateIsPublicValidator()
    {
       RuleFor(v => v.IsPublic).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("سرویس از نوع عمومی می باشد"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

