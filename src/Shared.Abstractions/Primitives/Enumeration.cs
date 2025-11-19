using System.Reflection;

namespace Shared.Abstractions.Primitives;

/// <summary>
/// Base class for type-safe enumeration pattern implementation.
/// Provides a strongly-typed alternative to standard enums with additional functionality
/// and better encapsulation of related behavior.
/// </summary>
/// <typeparam name="TEnum">The type of the enumeration class.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Enumeration{TEnum}"/> class.
/// </remarks>
/// <param name="value">The numeric value of the enumeration.</param>
/// <param name="name">The name of the enumeration.</param>
public abstract class Enumeration<TEnum>(int value, string name) : IEquatable<Enumeration<TEnum>>, IComparable<Enumeration<TEnum>>
    where TEnum : Enumeration<TEnum>
{
    private static readonly Lazy<TEnum[]> _enumerations = new(GetEnumerations);

    /// <summary>
    /// Gets the numeric value of the enumeration.
    /// </summary>
    public int Value { get; protected init; } = value;

    /// <summary>
    /// Gets the name of the enumeration.
    /// </summary>
    public string Name { get; protected init; } = name;

    /// <summary>
    /// Gets all defined enumeration values of type <typeparamref name="TEnum"/>.
    /// </summary>
    /// <returns>An array of all enumeration values.</returns>
    public static TEnum[] GetAll() => _enumerations.Value;

    /// <summary>
    /// Gets the enumeration value with the specified numeric value.
    /// </summary>
    /// <param name="value">The numeric value to search for.</param>
    /// <returns>The enumeration value with the specified numeric value.</returns>
    /// <exception cref="ArgumentException">Thrown when no enumeration with the specified value is found.</exception>
    public static TEnum FromValue(int value)
    {
        return GetAll().FirstOrDefault(enumeration => enumeration.Value == value) ??
               throw new ArgumentException($"No enumeration with value {value} found for type {typeof(TEnum).Name}.");
    }

    /// <summary>
    /// Gets the enumeration value with the specified name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <returns>The enumeration value with the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown when no enumeration with the specified name is found.</exception>
    public static TEnum FromName(string name)
    {
        return GetAll().FirstOrDefault(enumeration => string.Equals(enumeration.Name, name, StringComparison.OrdinalIgnoreCase)) ??
               throw new ArgumentException($"No enumeration with name '{name}' found for type {typeof(TEnum).Name}.");
    }

    /// <summary>
    /// Attempts to get the enumeration value with the specified numeric value.
    /// </summary>
    /// <param name="value">The numeric value to search for.</param>
    /// <param name="enumeration">When this method returns, contains the enumeration value if found; otherwise, null.</param>
    /// <returns><c>true</c> if an enumeration with the specified value was found; otherwise, <c>false</c>.</returns>
    public static bool TryFromValue(int value, out TEnum? enumeration)
    {
        enumeration = GetAll().FirstOrDefault(e => e.Value == value);
        return enumeration is not null;
    }

    /// <summary>
    /// Attempts to get the enumeration value with the specified name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="enumeration">When this method returns, contains the enumeration value if found; otherwise, null.</param>
    /// <returns><c>true</c> if an enumeration with the specified name was found; otherwise, <c>false</c>.</returns>
    public static bool TryFromName(string name, out TEnum? enumeration)
    {
        enumeration = GetAll().FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
        return enumeration is not null;
    }

    /// <summary>
    /// Determines whether the specified enumeration is equal to the current enumeration.
    /// </summary>
    /// <param name="other">The enumeration to compare with the current enumeration.</param>
    /// <returns><c>true</c> if the specified enumeration is equal to the current enumeration; otherwise, <c>false</c>.</returns>
    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetType() == other.GetType() && Value == other.Value;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current enumeration.
    /// </summary>
    /// <param name="obj">The object to compare with the current enumeration.</param>
    /// <returns><c>true</c> if the specified object is equal to the current enumeration; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Enumeration<TEnum> other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this enumeration.
    /// </summary>
    /// <returns>A hash code for the current enumeration.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Value);
    }

    /// <summary>
    /// Returns the name of the enumeration.
    /// </summary>
    /// <returns>The name of the enumeration.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Compares the current enumeration with another enumeration and returns an integer that indicates
    /// whether the current enumeration precedes, follows, or occurs in the same position in the sort order
    /// as the other enumeration.
    /// </summary>
    /// <param name="other">The enumeration to compare with the current enumeration.</param>
    /// <returns>A value that indicates the relative order of the enumerations being compared.</returns>
    public int CompareTo(Enumeration<TEnum>? other)
    {
        return other is null ? 1 : Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Determines whether two enumerations are equal.
    /// </summary>
    /// <param name="left">The first enumeration to compare.</param>
    /// <param name="right">The second enumeration to compare.</param>
    /// <returns><c>true</c> if the enumerations are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two enumerations are not equal.
    /// </summary>
    /// <param name="left">The first enumeration to compare.</param>
    /// <param name="right">The second enumeration to compare.</param>
    /// <returns><c>true</c> if the enumerations are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Enumeration<TEnum>? left, Enumeration<TEnum>? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Gets all enumeration values using reflection.
    /// </summary>
    /// <returns>An array of all enumeration values.</returns>
    private static TEnum[] GetEnumerations()
    {
        Type enumerationType = typeof(TEnum);

        return enumerationType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(fieldInfo => enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo => (TEnum)fieldInfo.GetValue(null)!)
            .OrderBy(enumeration => enumeration.Value)
            .ToArray();
    }
}