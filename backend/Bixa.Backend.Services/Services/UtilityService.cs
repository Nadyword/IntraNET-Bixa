namespace Bixa.Backend.Services.Services
{
    public static class UtilityService
    {
        /// <summary>
        /// Normalizes and formats a CI number to the format 000.000.000.
        /// If the value is null, empty, or non-numeric, returns string.Empty.
        /// </summary>
        /// <param name="ci">The CI value to format.</param>
        /// <returns>The CI in 000.000.000 format or string.Empty if invalid.</returns>
        public static string NormalizeCiFormat(string ci)
        {
            if (string.IsNullOrWhiteSpace(ci))
                return string.Empty;

            var ciTrimmed = ci.Trim();
            bool hasE = ciTrimmed.EndsWith("-E", StringComparison.OrdinalIgnoreCase);

            var digits = new string([.. ciTrimmed.Where(char.IsDigit)]);

            if (digits.Length == 0)
                return string.Empty;

            if (digits.Length > 9)
                digits = digits[^9..];

            var grupos = new List<string>();
            int resto = digits.Length % 3;
            if (resto > 0)
                grupos.Add(digits[..resto]);
            for (int i = resto; i < digits.Length; i += 3)
                grupos.Add(digits.Substring(i, 3));

            var formatted = string.Join('.', grupos);

            return hasE ? $"{formatted}-E" : formatted;
        }
    }
}