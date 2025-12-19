using ErrorOr;
using FluentValidation;
using MediatR;

namespace Shared.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that automatically validates MediatR requests using FluentValidation.
/// Runs validation before the request handler executes and returns validation errors if validation fails.
/// </summary>
/// <typeparam name="TRequest">The type of the MediatR request.</typeparam>
/// <typeparam name="TResponse">The inner type wrapped by ErrorOr.</typeparam>
/// <remarks>
/// Initializes a new instance of the ValidationBehavior class.
/// </remarks>
/// <param name="validators">The collection of validators for the request type.</param>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the request with validation.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ErrorOr result containing either the response or validation errors.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(request, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var errors = failures
            .Select(f => Error.Validation(
                code: f.PropertyName,
                description: f.ErrorMessage))
            .ToList();

        return (dynamic)errors;
    }
}