
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.DemoRequest_core.Commands.UpdateDemoRequest;

public class UpdateDemoRequestValidator : AbstractValidator<UpdateDemoRequestCommand>
{
    public UpdateDemoRequestValidator()
    {
       RuleFor(v => v.Accepted);
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
    }
}

