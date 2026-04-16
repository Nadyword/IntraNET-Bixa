namespace Bixa.Backend.Models.Validations;

public static class ValidationMessages
{
    public const string Required = "{PropertyName} es requerido.";
    public const string MaxLength = "{PropertyName} no puede exceder los {MaxLength} caracteres.";
    public const string MinLength = "{PropertyName} debe tener al menos {MinLength} caracteres.";
    public const string Invalid = "Formato inválido para {PropertyName}.";
    public const string InvalidEmail = "{PropertyName} tiene un formato de correo electrónico inválido.";
    public const string GreaterThan = "{PropertyName} debe ser mayor que {ComparisonValue}.";
    public const string InEnum = "{PropertyName} no es un valor válido.";
    public const string DateInPastOrPresent = "{PropertyName} debe ser una fecha en el pasado o presente.";
    public const string GreaterThanZero = "{PropertyName} debe ser mayor que cero.";
    public const string InvalidId = "{PropertyName} no es un identificador válido.";
    public const string InvalidTime = "El formato de hora para {PropertyName} es inválido.";
    public const string StartTimeBeforeEndTime = "La hora de inicio debe ser anterior a la hora de fin.";
    public const string InvalidEnum = "El valor proporcionado para {PropertyName} no es un miembro válido de su enumeración.";
    public const string NotDefaultValue = "El campo {PropertyName} debe tener un valor válido y no puede ser el valor por defecto (ej. fecha, hora).";
    public const string Range = "{PropertyName} debe estar entre {From} y {To}.";
    public const string LessThanOrEqualTo = "{PropertyName} debe ser menor o igual que {ComparisonValue}.";
    public const string LessThan = "{PropertyName} debe ser menor que {ComparisonValue}.";

    public static string InvalidEnumValue(Type enumType)
    {
        return $"{{PropertyName}} no es un valor válido para la enumeración {enumType.Name}.";
    }
}
