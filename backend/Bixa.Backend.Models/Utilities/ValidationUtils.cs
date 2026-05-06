using System.Text.RegularExpressions;

namespace Bixa.Backend.Models.Utilities;

/// <summary>
/// A utility class containing reusable static methods for common data validation logic.
/// </summary>
public static class ValidationUtils
{
    /// <summary>
    /// Validates a phone number based on character set and maximum length.
    /// </summary>
    /// <param name="phoneNumber">The phone number string to validate.</param>
    /// <returns>True if the phone number is valid; otherwise, false.</returns>
    public static bool IsValidPhone(string? phoneNumber)
    {
        const int MaxLength = 20;
        const string RegularExpressionPattern = @"^[0-9+\-() ]*$";

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        if (phoneNumber.Length > MaxLength)
            return false;

        return Regex.IsMatch(phoneNumber, RegularExpressionPattern);
    }

    /// <summary>
    /// Validates a password based on length and character complexity requirements.
    /// </summary>
    /// <param name="password">The password string to validate.</param>
    /// <returns>True if the password meets the complexity requirements; otherwise, false.</returns>
    public static List<string> IsValidPassword(string? password)
    {
        const int MinLength = 8;
        const int MaxLength = 20;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("La clave no puede estar vacía.");
            return errors;
        }

        if (password.Length < MinLength || password.Length > MaxLength)
            errors.Add($"Debe tener entre {MinLength} y {MaxLength} caracteres.");

        // 2. Mayúsculas
        if (!Regex.IsMatch(password, @"[A-Z]"))
            errors.Add("Debe contener al menos una letra mayúscula.");

        // 3. Minúsculas
        if (!Regex.IsMatch(password, @"[a-z]"))
            errors.Add("Debe contener al menos una letra minúscula.");

        // 4. Números
        if (!Regex.IsMatch(password, @"\d"))
            errors.Add("Debe contener al menos un número.");

        if (!Regex.IsMatch(password, @"[^\w]"))
            errors.Add("Debe contener al menos un carácter especial.");

        return errors;
    }
}