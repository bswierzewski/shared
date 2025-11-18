namespace Shared.Abstractions.Models;

/// <summary>
/// Represents an error with a code and descriptive message.
/// This record provides a strongly-typed way to handle errors in Result patterns.
/// </summary>
/// <param name="Code">The error code identifier.</param>
/// <param name="Message">The human-readable error message.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Gets an empty error instance.
    /// </summary>
    public static Error None => new(string.Empty, string.Empty);

    /// <summary>
    /// Creates an error with the specified code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new Error instance.</returns>
    public static Error Create(string code, string message) => new(code, message);

    /// <summary>
    /// Creates an error with only a message (empty code).
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new Error instance with empty code.</returns>
    public static Error FromMessage(string message) => new(string.Empty, message);

    /// <summary>
    /// Returns a string representation of the error.
    /// </summary>
    /// <returns>A formatted string containing the error code and message.</returns>
    public override string ToString() => 
        string.IsNullOrEmpty(Code) ? Message : $"{Code}: {Message}";
}