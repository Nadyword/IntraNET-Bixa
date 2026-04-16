using System.Linq.Expressions;
using Bixa.Backend.Models.DTOs.KeyValuePairModelDTO;

namespace Bixa.Backend.Models.Utilities;

public static class QueryableExtensions
{
    /// <summary>
    /// Applies dynamic filters to an IQueryable collection based on the properties of a filter object.
    /// This method constructs an Expression Tree at runtime to filter the query.
    /// </summary>
    /// <param name="query">The IQueryable collection to filter.</param>
    /// <param name="filters">An object whose properties and non-default values will be used as filters.
    /// Default values (null, empty strings, 0 for int, default for DateTime) are ignored.</param>
    /// <returns>A new IQueryable with the filters applied.</returns>
    /// <typeparam name="T">The type of the entity in the IQueryable collection.</typeparam>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, object filters)
    {
        if (filters == null)
            return query;

        var properties = filters.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(filters);

            bool isInvalidDateTime = false;
            Type propType = prop.PropertyType;

            if (propType == typeof(DateTime) && value is DateTime dtValue && dtValue == default)
                isInvalidDateTime = true;

            if (propType == typeof(DateTime?) && (value == null || (DateTime?)value == default))
                isInvalidDateTime = true;

            bool isInvalid = isInvalidDateTime
                || value == null
                || (value is string strValue && string.IsNullOrWhiteSpace(strValue))
                || (value is int intValue && intValue == 0);

            if (isInvalid)
                continue;

            var entityType = typeof(T);
            var entityProperty = entityType.GetProperty(prop.Name);

            if (entityProperty == null)
                continue;

            var param = Expression.Parameter(entityType, "x");
            var member = Expression.Property(param, entityProperty);

            // Asegurar que la Constant tenga el mismo tipo que la propiedad del entity (maneja Nullable y Enums)
            Type targetType = entityProperty.PropertyType;
            Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            object constantValue = value!;

            if (underlying.IsEnum && value != null)
            {
                if (value is string s)
                    constantValue = Enum.Parse(underlying, s);
                else
                    constantValue = Enum.ToObject(underlying, value);
            }
            else
            {

                try
                {
                    if (value != null && value.GetType() != underlying)
                        constantValue = Convert.ChangeType(value, underlying);
                }
                catch
                {

                }
            }

            var constant = Expression.Constant(constantValue, targetType);

            Expression body;
            if (entityProperty.PropertyType == typeof(string))
            {
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                if (containsMethod == null)
                    throw new InvalidOperationException("Could not find 'Contains' method on type 'string'.");
                body = Expression.Call(member, containsMethod, constant);
            }
            else
            {
                body = Expression.Equal(member, constant);
            }

            var expression = Expression.Lambda<Func<T, bool>>(body, param);
            query = query.Where(expression);
        }

        return query;
    }

    /// <summary>
    /// Dynamically projects a queryable collection into a list of Key-Value pairs.
    /// This method builds a 'Select' expression tree based on configurable property names.
    /// </summary>
    /// <param name="query">The IQueryable collection to project.</param>
    /// <param name="config">An object that specifies the property names to be used for the Key and Value.
    /// If not provided, it defaults to "Id" for the Key and "Name" for the Value.</param>
    /// <returns>An IQueryable of KeyValuePairDTO with the specified properties projected.</returns>
    /// <typeparam name="TEntity">The type of the entity in the IQueryable collection.</typeparam>
    public static IQueryable<KeyValuePairDTO<object, object>> ApplyKeyValueProjection<TEntity>(
        this IQueryable<TEntity> query,
        KeyFieldConfigurationDTO config)
    {
        // Determine the property names for Key and Value, with default fallbacks
        var keyPropertyName = string.IsNullOrWhiteSpace(config.KeyField) ? "Id" : config.KeyField;
        var valuePropertyName = string.IsNullOrWhiteSpace(config.ValueField) ? "Name" : config.ValueField;

        // Create the parameter for the expression tree (e => ...)
        var parameter = Expression.Parameter(typeof(TEntity), "e");

        // Access the properties dynamically (e.g., e.Id, e.Name)
        var keyProperty = Expression.Property(parameter, keyPropertyName);
        var valueProperty = Expression.Property(parameter, valuePropertyName);

        // Convert the properties to type 'object' for the generic DTO
        var keyAsObject = Expression.Convert(keyProperty, typeof(object));
        var valueAsObject = Expression.Convert(valueProperty, typeof(object));

        // Create a new instance of KeyValuePairDTO
        var newExpression = Expression.New(typeof(KeyValuePairDTO<object, object>));

        // Find the properties of the DTO to bind to
        var keyPropertyInfo = typeof(KeyValuePairDTO<object, object>).GetProperty("Key");
        var valuePropertyInfo = typeof(KeyValuePairDTO<object, object>).GetProperty("Value");

        if (keyPropertyInfo == null || valuePropertyInfo == null)
            throw new InvalidOperationException("Key or Value property not found on KeyValuePairDTO<object, object>.");

        // Bind the properties to the new expression
        var keyBinding = Expression.Bind(keyPropertyInfo, keyAsObject);
        var valueBinding = Expression.Bind(valuePropertyInfo, valueAsObject);

        // Initialize the new instance with the property bindings
        var memberInit = Expression.MemberInit(newExpression, keyBinding, valueBinding);

        // Create the lambda expression (e => new KeyValuePairDTO { Key = e.Prop1, Value = e.Prop2 })
        var selector = Expression.Lambda<Func<TEntity, KeyValuePairDTO<object, object>>>(memberInit, parameter);

        // Apply the Select expression to the query
        return query.Select(selector);
    }
}