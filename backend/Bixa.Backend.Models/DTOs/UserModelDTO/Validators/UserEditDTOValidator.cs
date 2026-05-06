using static Bixa.Backend.Models.Validations.ValidationMessages;
using FluentValidation;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.Models.Utilities;
using Bixa.Backend.Models.Configurations;

namespace Bixa.Backend.Models.DTOs.UserModelDTO.Validators;

public class UserEditDTOValidator : AbstractValidator<UserEditDTO>
{
    public UserEditDTOValidator()
    {
        RuleFor(user => user.Id)
            .IsValidId();

        When(user => !string.IsNullOrEmpty(user.Name), () =>
        {
            RuleFor(user => user.Name)
                .MaximumLength(ModelLengths.Name).WithMessage(MaxLength);
        });

        When(user => !string.IsNullOrEmpty(user.Email), () =>
        {
            RuleFor(x => x.Email!)
                .MaximumLength(ModelLengths.Email).WithMessage(MaxLength)
                .IsValidEmailFormat(ModelLengths.Email);
        });

        When(user => !string.IsNullOrEmpty(user.Phone), () =>
        {
            RuleFor(user => user.Phone!)
                .MaximumLength(ModelLengths.Phone).WithMessage(MaxLength)
                .IsValidPhoneNumberFormat();
        });

        When(user => user.IdUserRol.HasValue, () =>
        {
            RuleFor(user => user.IdUserRol!.Value)
                .IsValidId();
        });
    }
}