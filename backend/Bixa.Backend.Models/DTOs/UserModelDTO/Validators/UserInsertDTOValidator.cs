using Bixa.Backend.Models.Configurations;
using Bixa.Backend.Models.DTOs.UserModelDTO;
using Bixa.Backend.Models.Utilities;
using FluentValidation;

namespace Bixa.Backend.Models.DTOs.UserModelDTO.Validators;

public class UserInsertDTOValidator : AbstractValidator<UserInsertDTO>
{
    public UserInsertDTOValidator()
    {
        RuleFor(user => user.Name)
            .IsRequiredAndMaxLength(ModelLengths.Name);

        RuleFor(user => user.Email)
            .IsRequiredAndMaxLength(ModelLengths.Email)
            .IsValidEmailFormat(ModelLengths.Email);

        RuleFor(user => user.Phone)
            .IsRequiredAndMaxLength(ModelLengths.Phone)
            .IsValidPhoneNumberFormat();

        RuleFor(user => user.TaxId)
            .IsRequiredAndMaxLength(ModelLengths.TaxId)
            .IsValidTaxIdFormat();

        RuleFor(user => user.IdUserRol).IsValidId();
    }
}