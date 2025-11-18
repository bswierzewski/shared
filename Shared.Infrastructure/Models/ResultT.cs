namespace Shared.Infrastructure.Models;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with errors.
/// Provides a strongly-typed way to handle success and error cases with return values.
/// </summary>
/// <typeparam name="T">The type of the value returned on success.</typeparam>
public sealed class Result<T>
{
    /// <summary>
    /// Initializes a new instance of the Result{T} class.
    /// </summary>
    /// <param name="value">The value returned on success.</param>
    /// <param name="isSuccess">Indicates whether the operation succeeded.</param>
    /// <param name="errors">The collection of errors.</param>
    private Result(T? value, bool isSuccess, Error[] errors)
    {
        Value = value;
        IsSuccess = isSuccess;
        Errors = errors;
    }

    /// <summary>
    /// Gets the value returned on success.
    /// </summary>
    public T? Value { get; }

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
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value to return on success.</param>
    /// <returns>A successful result instance.</returns>
    public static Result<T> Success(T value) =>
        new(value, true, Array.Empty<Error>());

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed result instance.</returns>
    public static Result<T> Failure(params Error[] errors) =>
        new(default, false, errors);

    /// <summary>
    /// Creates a failed result with the specified errors.
    /// </summary>
    /// <param name="errors">The enumerable collection of errors.</param>
    /// <returns>A failed result instance.</returns>
    public static Result<T> Failure(IEnumerable<Error> errors) =>
        new(default, false, errors.ToArray());

    /// <summary>
    /// Creates a failed result with a single error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result instance.</returns>
    public static Result<T> Failure(string message) =>
        Failure(Error.FromMessage(message));

    /// <summary>
    /// Creates a failed result with a single error code and message.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A failed result instance.</returns>
    public static Result<T> Failure(string code, string message) =>
        Failure(Error.Create(code, message));

    /// <summary>
    /// Creates a result from a base Result instance.
    /// </summary>
    /// <param name="result">The base result.</param>
    /// <param name="value">The value to use if the result is successful.</param>
    /// <returns>A Result{T} instance.</returns>
    public static Result<T> From(Result result, T? value = default) =>
        result.IsSuccess
            ? Success(value!)
            : Failure(result.Errors);

    /// <summary>
    /// Implicit conversion from a value to a successful result.
    /// </summary>
    /// <param name="value">The value to wrap in a successful result.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from an Error to a failed result.
    /// </summary>
    /// <param name="error">The error to wrap in a failed result.</param>
    public static implicit operator Result<T>(Error error) => Failure(error);
}