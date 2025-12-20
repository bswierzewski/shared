using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Handler for ValidationCommand that returns a successful result if validation passes.
/// If validation fails, the ValidationBehavior will return validation errors before this handler is called.
/// </summary>
public class ValidationCommandHandler : IRequestHandler<ValidationCommand, ErrorOr<string>>
{
    /// <summary>
    /// Handles the ValidationCommand by returning a successful response.
    /// This handler is only reached if all validation rules pass.
    /// </summary>
    public Task<ErrorOr<string>> Handle(ValidationCommand request, CancellationToken cancellationToken)
    {
        var result = $"Validation passed! Name: {request.Name}, Email: {request.Email}, Age: {request.Age}";
        return Task.FromResult<ErrorOr<string>>(result);
    }
}
