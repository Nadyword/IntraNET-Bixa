using Bixa.Backend.Models.DTOs.UserModelDTO;
using FluentValidation;
using static Bixa.Backend.Models.Validations.ValidationMessages;

namespace Bixa.Backend.Models.DTOs.UserModelDTO.Validators;

public class UserChangePasswordDTOValidator : AbstractValidator<UserChangePasswordDTO>
{
    public UserChangePasswordDTOValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(Required)
            .GreaterThan(0).WithMessage(GreaterThan);
    }
}