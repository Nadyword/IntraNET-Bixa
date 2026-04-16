using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel;
using System.Reflection;

namespace Bixa.Backend.DataAccess.Converters;

public class EnumDescriptionNullableConverter<TEnum> : ValueConverter<TEnum?, string?> where TEnum : struct, Enum
{
    public EnumDescriptionNullableConverter() : base(
        enumValue => enumValue.HasValue ? GetEnumDescription(enumValue.Value) : null,
        stringValue => stringValue == null ? (TEnum?)null : GetEnumFromDescription(stringValue))
    {
    }

    public static string GetEnumDescription(TEnum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttribute<DescriptionAttribute>();
        return descriptionAttribute?.Description ?? value.ToString();
    }

    private static TEnum GetEnumFromDescription(string description)
    {
        var type = typeof(TEnum);
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            if (attribute?.Description == description)
                return (TEnum)field.GetValue(null)!;
            if (field.Name == description)
                return (TEnum)field.GetValue(null)!;
        }
        throw new ArgumentException($"No se encontró el valor del enum para la descripción '{description}'.");
    }
}