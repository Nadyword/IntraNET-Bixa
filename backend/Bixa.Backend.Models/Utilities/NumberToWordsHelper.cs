using System.Text;
namespace Bixa.Backend.Models.Utilities;

public static class NumberToWordsHelper
{
    public static string ToChileanCurrency(decimal amount)
    {
        long value = (long)Math.Floor(amount);
        if (value == 0) return "$0.- (Cero pesos).";

        string words = ConvertToWords(value).Trim();

        words = char.ToUpper(words[0]) + words.Substring(1).ToLower();

        return $"{value:C0}.- ({words} {(value == 1 ? "peso" : "pesos")}).";
    }

    private static string ConvertToWords(long n)
    {
        if (n < 0) return "Menos " + ConvertToWords(Math.Abs(n));
        if (n == 0) return "";

        if (n == 1) return "Un";
        if (n <= 19) return new[] { "", "Un", "Dos", "Tres", "Cuatro", "Cinco", "Seis", "Siete", "Ocho", "Nueve", "Diez", "Once", "Doce", "Trece", "Catorce", "Quince", "Dieciséis", "Diecisiete", "Dieciocho", "Diecinueve" }[n];

        if (n <= 99)
        {
            var decenas = new[] { "", "", "Veinte", "Treinta", "Cuarenta", "Cincuenta", "Sesenta", "Setenta", "Ochenta", "Noventa" };
            if (n == 20) return "Veinte";
            if (n < 30) return "Veinti" + ConvertToWords(n % 10).ToLower();
            return decenas[n / 10] + (n % 10 > 0 ? " y " + ConvertToWords(n % 10) : "");
        }

        if (n <= 999)
        {
            if (n == 100) return "Cien";
            var centenas = new[] { "", "Ciento", "Doscientos", "Trescientos", "Cuatrocientos", "Quinientos", "Seiscientos", "Setecientos", "Ochocientos", "Novecientos" };
            return centenas[n / 100] + (n % 100 > 0 ? " " + ConvertToWords(n % 100) : "");
        }

        if (n <= 999999)
        {
            if (n == 1000) return "Mil";
            string miles = (n < 2000) ? "Mil" : ConvertToWords(n / 1000) + " Mil";
            return miles + (n % 1000 > 0 ? " " + ConvertToWords(n % 1000) : "");
        }

        if (n <= 999999999999)
        {
            long millones = n / 1000000;
            long resto = n % 1000000;
            string palabraMillon = (millones == 1) ? "Un Millón" : ConvertToWords(millones) + " Millones";
            return palabraMillon + (resto > 0 ? " " + ConvertToWords(resto) : "");
        }

        return "Número demasiado grande";
    }
}