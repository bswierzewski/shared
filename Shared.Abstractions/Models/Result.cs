namespace Shared.Abstractions.Models;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// Provides a strongly-typed way to handle success and error cases.
/// </summary>
public sealed class Result
{
    /// <summary>
    /// Initializes a new instance of the Result class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="errors">The collection of errors.</param>
    private Result(bool isSuccess, Error[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the collection of errors.
    /// </summary>
    public IReadOnlyCollection<Error> Errors { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result instance.</returns>
    public static Result Success() =>
        new(true, Array.Empty<Error>());

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed result instance.</returns>
    public static Result Failure(params Error[] errors) =>
        new(false, errors);

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The enumerable collection of errors.</param>
    /// <returns>A failed result instance.</returns>
    public static Result Failure(IEnumerable<Error> errors) =>
        new(false, errors.ToArray());

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result instance.</returns>
    public static Result Failure(string message) =>
        Failure(Error.FromMessage(message));

    /// <summary>
    /// Creates a failed result with a single error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result instance.</returns>
    public static Result Failure(string code, string message) =>
        Failure(Error.Create(code, message));
}