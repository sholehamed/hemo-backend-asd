
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.SubSystem_core.Commands.SubSystemUpdateSubSystemIsActive;

public class SubSystemUpdateSubSystemIsActiveValidator : AbstractValidator<SubSystemUpdateSubSystemIsActiveCommand>
{
    public SubSystemUpdateSubSystemIsActiveValidator()
    {
       RuleFor(v => v.Active).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("فغال"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

