
using Application.Common.Utility.Validator;
namespace Application.Core.User_core.Commands.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
       RuleFor(v => v.Username).MaximumLength(64).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام کاربری",16)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نام کاربری"));
       RuleFor(v => v.Password).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("رمز عبور",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("رمز عبور"));
       RuleFor(v => v.FirstName).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نام"));
       RuleFor(v => v.LastName).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("نام خانوادگی",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("نام خانوادگی"));
       RuleFor(v => v.Mobile).MaximumLength(16).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("موبایل",16));
       RuleFor(v => v.Email).MaximumLength(64).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("ایمیل",64));

    }
}

