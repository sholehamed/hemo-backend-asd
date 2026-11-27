
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.OrgLocationType_core.Commands.UpdateOrgLocationType;

public class UpdateOrgLocationTypeValidator : AbstractValidator<UpdateOrgLocationTypeCommand>
{
    public UpdateOrgLocationTypeValidator()
    {
       RuleFor(v => v.Name).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("عنوان",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("عنوان"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

