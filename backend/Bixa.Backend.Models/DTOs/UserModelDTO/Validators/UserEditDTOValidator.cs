using static Bixa.Backend.Models.Validations.ValidationMessages;
using Bixa.Backend.Models.Configurations;
using Bixa.Backend.Models.Utilities;
using FluentValidation;

namespace Bixa.Backend.Models.DTOs.UserModelDTO.Validators;

public class UserEditDTOValidator : AbstractValidator<UserEditDTO>
{
    public UserEditDTOValidator()
    {
        When(user => user.IdUserRol.HasValue, () =>
        {
            RuleFor(user => user.IdUserRol!.Value)
                .IsValidId();
        });
    }
}