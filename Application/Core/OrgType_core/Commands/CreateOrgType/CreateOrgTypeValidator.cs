
using Application.Common.Utility.Validator;
namespace Application.Core.OrgType_core.Commands.CreateOrgType;

public class CreateOrgTypeValidator : AbstractValidator<CreateOrgTypeCommand>
{
    public CreateOrgTypeValidator()
    {
       RuleFor(v => v.OrgLocationTypeGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نوع شرکت"));
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));

    }
}

