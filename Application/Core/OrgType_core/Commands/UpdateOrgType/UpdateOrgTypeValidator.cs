
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.OrgType_core.Commands.UpdateOrgType;

public class UpdateOrgTypeValidator : AbstractValidator<UpdateOrgTypeCommand>
{
    public UpdateOrgTypeValidator()
    {
       RuleFor(v => v.OrgLocationTypeGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نوع شرکت"));
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

