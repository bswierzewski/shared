using System.ComponentModel;
using System.Reflection;

namespace Shared.Abstractions.Extensions;

/// <summary>
/// Provides extension methods for Enum types to extract metadata and descriptions.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description of an enum value from the DescriptionAttribute if present.
    /// </summary>
    /// <param name="value">The enum value to get description for.</param>
    /// <param name="useNameAsFallback">If true, returns enum name when DescriptionAttribute is not present. Default is false.</param>
    /// <returns>Description from DescriptionAttribute, enum name (if useNameAsFallback is true), or null.</returns>
    public static string? GetEnumDescription(this Enum? value, bool useNameAsFallback = false)
    {
        if (value == null)
            return null;

        FieldInfo? fi = value.GetType().GetField(value.ToString());

        if (fi != null)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;
        }

        return useNameAsFallback ? value.ToString() : null;
    }
}