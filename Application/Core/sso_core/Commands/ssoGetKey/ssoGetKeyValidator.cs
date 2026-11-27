
using Application.Common.Utility.Validator;
namespace Application.Core.sso_core.Commands.ssoGetKey;

public class ssoGetKeyValidator : AbstractValidator<ssoGetKeyCommand>
{
    public ssoGetKeyValidator()
    {
       RuleFor(v => v.PackageGUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پکیج"));
 
    }
}

