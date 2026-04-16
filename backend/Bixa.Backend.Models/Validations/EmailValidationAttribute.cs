using System.ComponentModel.DataAnnotations;


namespace Bixa.Backend.Models.Validations;

/// <summary>
/// Custom validation attribute for email addresses.
/// Encapsulates common email validation requirements: Required, EmailAddress format, and MaxLength.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class EmailValidationAttribute : ValidationAttribute
{
    // Default error messages for failed validation.
    private const string DefaultRequiredErrorMessage = "Email is required.";
    private const string DefaultEmailFormatErrorMessage = "Please enter a valid email address format.";
    private const string DefaultMaxLengthErrorMessage = "Email address cannot exceed {0} characters.";

    /// <summary>
    /// Gets or sets a value indicating whether the email address is required.
    /// Default is true.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum allowed length of the email address string.
    /// Default is 255 characters.
    /// </summary>
    public int MaxLength { get; set; } = 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailValidationAttribute"/> class.
    /// Uses a base constructor, though specific error messages are handled within IsValid.
    /// </summary>
    public EmailValidationAttribute() : base("Invalid email address.") { }

    /// <summary>
    /// Validates the specified value against the email address requirements.
    /// </summary>
    /// <param name="value">The email address string to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>An instance of the <see cref="ValidationResult"/> class.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // 1. Handle Required validation
        // Creates an instance of the built-in RequiredAttribute and checks if the value is null or empty.
        // This ensures the field is present if IsRequired is true.
        if (IsRequired)
        {
            var requiredAttribute = new RequiredAttribute { ErrorMessage = DefaultRequiredErrorMessage };
            if (!requiredAttribute.IsValid(value))
                return new ValidationResult(requiredAttribute.ErrorMessage, new[] { validationContext.MemberName! });
        }
        else // If not required, and value is null or empty, it's considered valid.
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        string? email = value?.ToString();// At this point, email should not be null if IsRequired was true.
        if (email == null)
            return new ValidationResult(DefaultRequiredErrorMessage, new[] { validationContext.MemberName! });


        // 2. Handle MaxLength validation
        // Checks if the email string exceeds the allowed MaxLength.
        if (email.Length > MaxLength)
            return new ValidationResult(string.Format(DefaultMaxLengthErrorMessage, MaxLength), new[] { validationContext.MemberName! });

        // 3. Handle EmailAddress format validation
        // Creates an instance of the built-in EmailAddressAttribute and checks the format.
        // This leverages Microsoft's robust regex for email format validation.
        var emailAddressAttribute = new EmailAddressAttribute { ErrorMessage = DefaultEmailFormatErrorMessage };
        if (!emailAddressAttribute.IsValid(email))
            return new ValidationResult(emailAddressAttribute.ErrorMessage, new[] { validationContext.MemberName! });

        return ValidationResult.Success;
    }
}