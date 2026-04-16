using System.ComponentModel;
using System.Reflection;

namespace Bixa.Backend.Models.Utilities;

public static class EnumExtensions
{
    /// <summary>
    /// Gets the value of a property from an object generically.
    /// </summary>
    /// <typeparam name="T">The data type of the property value. Can be a nullable type (e.g., string?, int?).</typeparam>
    /// <param name="obj">The object from which to get the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The value of the property, or the default value of T if the property is null or does not exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the object or property name is null or empty.</exception>
    /// <exception cref="InvalidCastException">Thrown if the property value cannot be converted to type T.</exception>
    /// <exception cref="OverflowException">Thrown if the property value is outside the range of type T.</exception>
    /// <exception cref="FormatException">Thrown if the property value is not in the correct format for type T.</exception>
    public static T? GetPropertyValue<T>(this object obj, string propertyName)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        PropertyInfo? propertyInfo = obj.GetType().GetProperty(propertyName);

        if (propertyInfo == null)
        {
            return default;
        }

        object? value = propertyInfo.GetValue(obj, null);

        if (value == null) return default;

        if (value is T directValue) return directValue;

        if (typeof(T).IsEnum && value is string stringValue)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), stringValue, ignoreCase: true);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCastException($"Could not convert '{stringValue}' to enum of type '{typeof(T).Name}'. {ex.Message}", ex);
            }
        }

        TypeConverter? converter = TypeDescriptor.GetConverter(typeof(T));
        if (converter != null && converter.CanConvertFrom(value.GetType()))
        {
            try
            {
                return (T?)converter.ConvertFrom(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Error converting '{propertyName}' from '{value.GetType().Name}' to '{typeof(T).Name}' using TypeConverter. {ex.Message}", ex);
            }
        }

        if (value is IConvertible)
        {
            try
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Error converting '{propertyName}' from '{value.GetType().Name}' to '{typeof(T).Name}' using Convert.ChangeType. {ex.Message}", ex);
            }
        }

        throw new InvalidCastException($"Cannot convert property '{propertyName}' of type '{value.GetType().Name}' to type '{typeof(T).Name}'.");
    }

    /// <summary>
    /// Returns the description attribute associated with the specified enum value,
    /// or the enum value's name if no description attribute is found.
    /// </summary>
    /// <param name="enumValue">The enum value to get the description for.</param>
    /// <returns>The description attribute of the enum value, or the enum value's name.</returns>
    public static string GetDescription(this Enum enumValue)
    {
        FieldInfo? fi = enumValue.GetType().GetField(enumValue.ToString());

        if (fi == null)
            return enumValue.ToString();

        DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[] ?? Array.Empty<DescriptionAttribute>();

        if (attributes.Length > 0)
            return attributes[0].Description;
        else
            return enumValue.ToString();
    }
}