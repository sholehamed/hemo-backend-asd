
using Application.Common.Utility.Validator;
namespace Application.Core.DemoRequest_core.Commands.CreateDemoRequest;

public class CreateDemoRequestValidator : AbstractValidator<CreateDemoRequestCommand>
{
    public CreateDemoRequestValidator()
    {
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
       RuleFor(v => v.OrgLocationGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("شرکت"));
    }
}

