using FluentValidation;
using static Bixa.Backend.Models.Validations.ValidationMessages;
using static Bixa.Backend.Models.Utilities.ValidationUtils;

namespace Bixa.Backend.Models.Utilities;

public static class ValidationRuleExtensions
{
    /// <summary>
    /// Validates that an integer ID is not empty/default and is greater than zero.
    /// </summary>
    public static IRuleBuilderOptions<T, int> IsValidId<T>(this IRuleBuilder<T, int> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage(Required)
            .GreaterThan(0).WithMessage("'{PropertyName}' must be a valid ID greater than zero.");

    /// <summary>
    /// Validates that a string property is not empty and respects a maximum length.
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsRequiredAndMaxLength<T>(this IRuleBuilder<T, string> ruleBuilder, int maxLength) =>
        ruleBuilder
            .NotEmpty().WithMessage(Required)
            .MaximumLength(maxLength).WithMessage(MaxLength);

    /// <summary>
    /// Validates that a string property is a valid email format and respects a maximum length.
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidEmailFormat<T>(this IRuleBuilder<T, string> ruleBuilder, int maxLength) =>
        ruleBuilder
            .EmailAddress().WithMessage(InvalidEmail)
            .MaximumLength(maxLength).WithMessage(MaxLength);

    /// <summary>
    /// Validates that a string property adheres to the defined phone number format using ValidationUtils.
    /// </summary>
    public static IRuleBuilderOptions<T, string> IsValidPhoneNumberFormat<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(IsValidPhone).WithMessage(Invalid);

    /// <summary>
    /// Validates that a DateTime value has its Kind property set to Utc.
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> IsUtc<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .Must(date => date.Kind == DateTimeKind.Utc)
            .WithMessage("{PropertyName} must be a UTC date.");
    }
}