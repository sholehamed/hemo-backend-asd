
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.UserLogoutByAdmin;

public class UserLogoutByAdminValidator : AbstractValidator<UserLogoutByAdminCommand>
{
    public UserLogoutByAdminValidator()
    {

       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));

    }
}

