using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowValidationException;

/// <summary>
/// Handler for ThrowValidationExceptionCommand.
/// Note: This handler will typically not be reached because ValidationBehavior will throw ValidationException
/// before the handler executes when invalid data is provided.
/// </summary>
public class ThrowValidationExceptionHandler : IRequestHandler<ThrowValidationExceptionCommand, Unit>
{
    /// <inheritdoc />
    public Task<Unit> Handle(ThrowValidationExceptionCommand request, CancellationToken cancellationToken)
    {
        // This line should not be reached when validation fails
        // ValidationBehavior intercepts and throws ValidationException before handler execution
        return Task.FromResult(Unit.Value);
    }
}
