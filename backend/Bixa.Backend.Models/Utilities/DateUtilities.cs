namespace Bixa.Backend.Models.Utilities;

public static class DateUtilities
{
    public static IEnumerable<DateTime> DatesUntil(this DateTime startDate, DateTime endDate)
    {
        for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        {
            yield return date;
        }
    }

    public static int GetdaysInaRange(this DateTime startDate, DateTime endDate)
    {
        return GetdaysInaRange(startDate, endDate, false);
    }

    public static int GetdaysInaRange(this DateTime startDate, DateTime endDate, bool includeWeekendDays = false)
    {
        var daysInaRage = startDate.DatesUntil(endDate);
        return daysInaRage.Count() - (includeWeekendDays ? daysInaRage.Where(d => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday).Count() : 0);
    }

    /// <summary>
    /// Generates a secure random password with a mix of upper and lower case letters, digits, and symbols.
    /// </summary>
    /// <param name="length">The desired length of the password. Default is 12 characters.</param>
    /// <returns>A secure random password as a string.</returns>
    public static string GenerateSecureRandomPassword(int length = 12)
    {
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specials = "!@#$%^&*()-_=+[]{}|;:,.<>?";
        var random = new Random();

        // Garantizar al menos un carácter de cada tipo
        var passwordChars = new List<char>
        {
            lower[random.Next(lower.Length)],
            upper[random.Next(upper.Length)],
            digits[random.Next(digits.Length)],
            specials[random.Next(specials.Length)]
        };

        // Rellenar el resto de la contraseña
        string allChars = lower + upper + digits + specials;
        for (int i = passwordChars.Count; i < length; i++)
        {
            passwordChars.Add(allChars[random.Next(allChars.Length)]);
        }

        // Mezclar los caracteres para evitar patrones predecibles
        passwordChars = passwordChars.OrderBy(_ => random.Next()).ToList();

        return new string(passwordChars.ToArray());
    }
}