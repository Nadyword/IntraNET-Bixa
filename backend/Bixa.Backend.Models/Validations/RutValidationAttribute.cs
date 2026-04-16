using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bixa.Backend.Models.Validations;

public class RutValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return new ValidationResult("RUT cannot be empty.");

        string rut = value.ToString()!.Trim().ToUpper();

        if (!Regex.IsMatch(rut, @"^\d{1,2}(\.\d{3}){2}-[\dK]$"))
            return new ValidationResult("Invalid format. Must be: XX.XXX.XXX-Y");

        string rutLimpio = rut.Replace(".", "").Replace("-", "");
        char dvIngresado = rutLimpio[^1];
        string cuerpo = rutLimpio[..^1];

        if (cuerpo.Length is not (7 or 8))
            return new ValidationResult("Invalid numeric length (must have 7 or 8 digits).");


        if (!long.TryParse(cuerpo, out _))
            return new ValidationResult("Contains non-numeric characters before the hyphen.");

        int suma = 0;
        int multiplicador = 2;

        for (int i = cuerpo.Length - 1; i >= 0; i--)
        {
            suma += int.Parse(cuerpo[i].ToString()) * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        int dvCalculado = 11 - (suma % 11);
        char dvEsperado = dvCalculado switch
        {
            11 => '0',
            10 => 'K',
            _ => dvCalculado.ToString()[0]
        };

        if (dvEsperado != dvIngresado)
            return new ValidationResult($"Invalid verifying digit. Should be: {dvEsperado}");

        return ValidationResult.Success;
    }
}