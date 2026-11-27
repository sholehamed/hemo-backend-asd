
using Application.Common.Mappings;
using Domain.Entities;
using Application.Common.Utility.Validator;
namespace Application.Core.PasswordBlockList_core.Commands.UpdatePasswordBlockList;

public class UpdatePasswordBlockListValidator : AbstractValidator<UpdatePasswordBlockListCommand>
{
    public UpdatePasswordBlockListValidator()
    {
       RuleFor(v => v.Password).MaximumLength(256).WithMessage(ValidatorMassege.GetMaxLengthErrorMessage("پسورد",256)).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("پسورد"));
       RuleFor(v => v.GUID).NotNull().WithMessage(ValidatorMassege.GetNotEmptyErrorMessage("کلید"));
       

    }
}

