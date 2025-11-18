using Shared.Abstractions.Models;

namespace Shared.Abstractions.Extensions;

/// <summary>
/// Extension methods for Result and Result{T} to support functional programming patterns.
/// These extensions enable method chaining, pattern matching, and composition of operations.
/// </summary>
public static class ResultExtensions
{
    #region Result (non-generic) Extensions

    /// <summary>
    /// Executes one of two functions based on the result state and returns the result.
    /// </summary>
    /// <typeparam name="TOut">The type of the return value.</typeparam>
    /// <param name="result">The result to match against.</param>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<IReadOnlyCollection<Error>, TOut> onFailure) =>
        result.IsSuccess ? onSuccess() : onFailure(result.Errors);

    /// <summary>
    /// Chains another Result operation if the current result is successful.
    /// </summary>
    /// <param name="result">The current result.</param>
    /// <param name="next">Function to execute if the current result is successful.</param>
    /// <returns>The result of the next operation or the current failed result.</returns>
    public static Result Bind(
        this Result result,
        Func<Result> next) =>
        result.IsSuccess ? next() : result;

    /// <summary>
    /// Chains a Result{T} operation if the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the value in the target result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="next">Function to execute if the current result is successful.</param>
    /// <returns>The result of the next operation or a failed Result{T}.</returns>
    public static Result<T> Bind<T>(
        this Result result,
        Func<Result<T>> next) =>
        result.IsSuccess ? next() : Result<T>.Failure(result.Errors);

    /// <summary>
    /// Converts a Result to Result{T} with the specified value if successful.
    /// </summary>
    /// <typeparam name="T">The type of the value to include.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="value">The value to include on success.</param>
    /// <returns>A Result{T} instance.</returns>
    public static Result<T> WithValue<T>(this Result result, T value) =>
        Result<T>.From(result, value);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result OnSuccess(this Result result, Action onSuccess)
    {
        if (result.IsSuccess)
            onSuccess();
        return result;
    }

    /// <summary>
    /// Executes an action if the result is failed.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="onFailure">Action to execute on failure.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result OnFailure(this Result result, Action<IReadOnlyCollection<Error>> onFailure)
    {
        if (result.IsFailure)
            onFailure(result.Errors);
        return result;
    }

    #endregion

    #region Result<T> Extensions

    /// <summary>
    /// Executes one of two functions based on the result state and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <typeparam name="TOut">The type of the return value.</typeparam>
    /// <param name="result">The result to match against.</param>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the executed function.</returns>
    public static TOut Match<T, TOut>(
        this Result<T> result,
        Func<T, TOut> onSuccess,
        Func<IReadOnlyCollection<Error>, TOut> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value!) : onFailure(result.Errors);

    /// <summary>
    /// Transforms the value inside a successful result using the provided function.
    /// </summary>
    /// <typeparam name="T">The type of the current value.</typeparam>
    /// <typeparam name="U">The type of the transformed value.</typeparam>
    /// <param name="result">The result containing the value to transform.</param>
    /// <param name="mapper">Function to transform the value.</param>
    /// <returns>A new result with the transformed value or the original errors.</returns>
    public static Result<U> Map<T, U>(
        this Result<T> result,
        Func<T, U> mapper) =>
        result.IsSuccess ? Result<U>.Success(mapper(result.Value!)) : Result<U>.Failure(result.Errors);

    /// <summary>
    /// Chains another Result{U} operation if the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the current value.</typeparam>
    /// <typeparam name="U">The type of the value in the target result.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="binder">Function to execute if the current result is successful.</param>
    /// <returns>The result of the next operation or a failed Result{U}.</returns>
    public static Result<U> Bind<T, U>(
        this Result<T> result,
        Func<T, Result<U>> binder) =>
        result.IsSuccess ? binder(result.Value!) : Result<U>.Failure(result.Errors);

    /// <summary>
    /// Chains a Result operation if the current result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the current value.</typeparam>
    /// <param name="result">The current result.</param>
    /// <param name="binder">Function to execute if the current result is successful.</param>
    /// <returns>The result of the next operation or a failed Result.</returns>
    public static Result Bind<T>(
        this Result<T> result,
        Func<T, Result> binder) =>
        result.IsSuccess ? binder(result.Value!) : Result.Failure(result.Errors);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> onSuccess)
    {
        if (result.IsSuccess)
            onSuccess(result.Value!);
        return result;
    }

    /// <summary>
    /// Executes an action if the result is failed.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="onFailure">Action to execute on failure.</param>
    /// <returns>The original result for chaining.</returns>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<IReadOnlyCollection<Error>> onFailure)
    {
        if (result.IsFailure)
            onFailure(result.Errors);
        return result;
    }

    /// <summary>
    /// Converts a Result{T} to a base Result, discarding the value.
    /// </summary>
    /// <typeparam name="T">The type of the value in the result.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>A base Result instance.</returns>
    public static Result ToResult<T>(this Result<T> result) =>
        result.IsSuccess ? Result.Success() : Result.Failure(result.Errors);

    #endregion

    #region Utility Extensions

    /// <summary>
    /// Ensures that a value is not null, returning a failed result if it is.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="errorMessage">The error message to use if the value is null.</param>
    /// <returns>A successful result with the value or a failed result.</returns>
    public static Result<T> EnsureNotNull<T>(this T? value, string errorMessage = "Value cannot be null")
        where T : class =>
        value is not null ? Result<T>.Success(value) : Result<T>.Failure(errorMessage);

    /// <summary>
    /// Ensures that a value meets the specified condition, returning a failed result if it doesn't.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result containing the value to validate.</param>
    /// <param name="predicate">The condition to check.</param>
    /// <param name="errorMessage">The error message to use if the condition is not met.</param>
    /// <returns>The original result if successful and condition is met, or a failed result.</returns>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        string errorMessage) =>
        result.IsSuccess && predicate(result.Value!)
            ? result
            : Result<T>.Failure(errorMessage);

    #endregion
}