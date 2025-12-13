#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Shared.EnvFileGenerator.Services;

/// <summary>
/// Formats property values for environment variables.
/// </summary>
internal class PropertyValueFormatter
{
    /// <summary>
    /// Gets the default value for a property type.
    /// </summary>
    public string GetDefaultValue(Type type)
    {
        if (type == typeof(string)) return "";
        if (type == typeof(bool)) return "false";
        if (type.IsValueType && type != typeof(Guid) && type != typeof(TimeSpan)) return "0";
        if (type == typeof(Guid)) return Guid.Empty.ToString();
        if (type == typeof(TimeSpan)) return "00:00:00";
        return "";
    }

    /// <summary>
    /// Gets the actual property value from an instance, formatted as string.
    /// </summary>
    public string GetPropertyDefaultValue(object? instance, System.Reflection.PropertyInfo property)
    {
        try
        {
            if (instance != null)
            {
                var value = property.GetValue(instance);
                return FormatPropertyValue(value, property.PropertyType);
            }
        }
        catch
        {
            // If reading fails, fall back to empty
        }

        return "";
    }

    /// <summary>
    /// Formats a property value as a string for environment variables.
    /// </summary>
    public string FormatPropertyValue(object? value, Type propertyType)
    {
        if (value == null)
            return "";

        // Handle nullable types
        if (Nullable.GetUnderlyingType(propertyType) != null)
        {
            if (value == null)
                return "";
        }

        // Format the value as string
        if (propertyType == typeof(bool))
            return value.ToString()!.ToLowerInvariant();

        if (propertyType == typeof(string))
        {
            var strValue = value.ToString();
            return string.IsNullOrEmpty(strValue) ? "" : strValue;
        }

        if (propertyType.IsValueType)
        {
            // Check if it's a default value (0, false, etc.)
            var typeDefault = Activator.CreateInstance(propertyType);
            if (value.Equals(typeDefault))
                return "";
            return value.ToString() ?? "";
        }

        return value.ToString() ?? "";
    }

    /// <summary>
    /// Gets the friendly type name for display.
    /// </summary>
    public string GetTypeName(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return $"{GetTypeName(type.GetGenericArguments()[0])}?";
        if (type.IsArray) return $"{GetTypeName(type.GetElementType()!)}[]";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return $"List<{GetTypeName(type.GetGenericArguments()[0])}>";

        return type.Name switch
        {
            "String" => "string",
            "Int32" => "int",
            "Int64" => "long",
            "Boolean" => "bool",
            "Double" => "double",
            "Decimal" => "decimal",
            _ => type.Name
        };
    }

    /// <summary>
    /// Checks if a type is simple (primitive or known simple types).
    /// </summary>
    public bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive) return true;

        return type == typeof(string) ||
               type == typeof(Guid) ||
               type == typeof(TimeSpan) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(decimal) ||
               type == typeof(byte[]) ||
               Nullable.GetUnderlyingType(type) is not null && IsSimpleType(Nullable.GetUnderlyingType(type)!);
    }

    /// <summary>
    /// Checks if a type is complex (needs recursive processing).
    /// </summary>
    public bool IsComplexType(Type type)
    {
        if (type.IsPrimitive || type.IsArray || type == typeof(string))
            return false;

        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef == typeof(List<>) || genericDef == typeof(Dictionary<,>) ||
                genericDef == typeof(Nullable<>) || genericDef == typeof(IEnumerable<>) ||
                genericDef == typeof(ICollection<>))
                return false;
        }

        var properties = GetPublicProperties(type);
        return properties.Count > 0 && type.IsClass && !type.IsAbstract;
    }

    /// <summary>
    /// Creates an instance of a type safely.
    /// </summary>
    public object? CreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets public readable/writable properties of a type.
    /// </summary>
    public List<System.Reflection.PropertyInfo> GetPublicProperties(Type type)
    {
        return type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                   .Where(p => p.CanRead && p.CanWrite)
                   .OrderBy(p => p.Name)
                   .ToList();
    }
}
