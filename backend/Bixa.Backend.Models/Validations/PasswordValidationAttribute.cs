using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bixa.Backend.Models.Validations;

/// <summary>
/// Custom validation attribute for strong password requirements.
/// Ensures the password meets criteria for length and character composition.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PasswordValidationAttribute : ValidationAttribute
{
    // Default error message for failed validation.
    // This can be overridden when applying the attribute (e.g., [PasswordValidation(ErrorMessage = "Custom message")]).
    private const string DefaultErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.";

    /// <summary>
    /// Gets or sets the minimum allowed length of the password.
    /// Default is 8 characters.
    /// </summary>
    public int MinLength { get; set; } = 8;

    /// <summary>
    /// Gets or sets the maximum allowed length of the password.
    /// Default is 20 characters.
    /// </summary>
    public int MaxLength { get; set; } = 20;

    /// <summary>
    /// Regular expression pattern for strong password validation.
    /// Requirements:
    /// ^                   - Start of the string.
    /// (?=.*[a-z])         - Must contain at least one lowercase letter.
    /// (?=.*[A-Z])         - Must contain at least one uppercase letter.
    /// (?=.*\d)            - Must contain at least one digit (number 0-9).
    /// .{"MinLength",}     - Must be at least MinLength characters long (handled by MinLength property).
    /// .$                  - End of the string.
    /// Note: Length is primarily handled by MinLength and MaxLength properties.
    /// </summary>
    private string RegularExpressionPattern => @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{" + MinLength + "," + MaxLength + "}$";

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordValidationAttribute"/> class.
    /// </summary>
    public PasswordValidationAttribute() : base(DefaultErrorMessage) { }

    /// <summary>
    /// Validates the specified value with respect to the current validation attribute.
    /// This method is the core logic for the custom validation.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>An instance of the <see cref="ValidationResult"/> class.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 1. Check for null or empty password
        // If the value is null or consists only of white-space characters, it's considered invalid.
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("Password cannot be empty.", new[] { validationContext.MemberName! });

        string password = value.ToString()!;

        // 2. Validate password length
        // Checks if the password length is within the specified minimum and maximum bounds.
        if (password.Length < MinLength || password.Length > MaxLength)
            return new ValidationResult($"Password length must be between {MinLength} and {MaxLength} characters.", new[] { validationContext.MemberName! });

        // 3. Validate password complexity using Regular Expression
        // Applies the regex pattern to ensure the password contains at least one lowercase, one uppercase, and one digit.
        if (!Regex.IsMatch(password, RegularExpressionPattern))
            return new ValidationResult(DefaultErrorMessage, new[] { validationContext.MemberName! });

        // If all validations pass, return success.
        return ValidationResult.Success;
    }
}