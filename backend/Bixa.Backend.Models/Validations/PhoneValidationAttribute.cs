using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bixa.Backend.Models.Validations;

/// <summary>
/// Custom validation attribute for phone numbers.
/// Ensures the phone number adheres to a specified maximum length and contains only valid characters.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PhoneValidationAttribute : ValidationAttribute
{
    // Default error message for invalid characters based on the regex.
    private const string InvalidCharactersErrorMessage = "Phone number contains invalid characters. Only digits, +, -, (, ), and spaces are allowed.";

    /// <summary>
    /// Gets or sets the maximum allowed length of the phone number string.
    /// Default is 12 characters, as specified.
    /// </summary>
    public int MaxLength { get; set; } = 12;

    /// <summary>
    /// Regular expression pattern to validate phone number characters.
    /// Requirements:
    /// ^                   - Start of the string.
    /// [0-9+\-() ]* - Allows zero or more occurrences of:
    ///                     - Digits (0-9)
    ///                     - Plus sign (+)
    ///                     - Hyphen (-)
    ///                     - Opening parenthesis (
    ///                     - Closing parenthesis )
    ///                     - Space ( )
    /// $                   - End of the string.
    /// </summary>
    private const string RegularExpressionPattern = @"^[0-9+\-() ]*$";

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneValidationAttribute"/> class.
    /// Uses a base constructor with a default error message for general validation failures,
    /// though more specific messages are provided within IsValid for clarity.
    /// </summary>
    public PhoneValidationAttribute() : base(InvalidCharactersErrorMessage) { }

    /// <summary>
    /// Validates the specified value against the phone number requirements.
    /// </summary>
    /// <param name="value">The phone number string to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>An instance of the <see cref="ValidationResult"/> class.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 1. Handle null or empty values
        // If the phone number is optional, a null or empty string might be considered valid here.
        // If it's mandatory, a [Required] attribute should be used in conjunction with this attribute.
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            // If the field is optional, return success. If it's required, [Required] attribute will handle it.
            return ValidationResult.Success;

        string phoneNumber = value.ToString()!;

        // 2. Validate maximum length
        // Checks if the phone number string exceeds the allowed MaxLength.
        if (phoneNumber.Length > MaxLength)
            return new ValidationResult($"Phone number cannot exceed {MaxLength} characters.", new[] { validationContext.MemberName! });

        // 3. Validate characters using Regular Expression
        // Ensures that the phone number only contains digits, plus sign, hyphen, parentheses, and spaces.
        if (!Regex.IsMatch(phoneNumber, RegularExpressionPattern))
            return new ValidationResult(InvalidCharactersErrorMessage, new[] { validationContext.MemberName! });

        // If all validations pass, return success.
        return ValidationResult.Success;
    }
}