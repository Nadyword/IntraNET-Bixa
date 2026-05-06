using Bixa.Backend.Models.Configurations;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.Models.Utilities;
using FluentValidation;

namespace Bixa.Backend.Models.DTOs.UserModelDTO.Validators;

public class UserInsertDTOValidator : AbstractValidator<UserInsertDTO>
{
    public UserInsertDTOValidator()
    {
        RuleFor(user => user.IdUserRol).IsValidId();
    }
}