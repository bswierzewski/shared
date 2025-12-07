using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowUnauthorizedException;

/// <summary>
/// Handler that throws an UnauthorizedAccessException.
/// </summary>
public class ThrowUnauthorizedExceptionHandler : IRequestHandler<ThrowUnauthorizedExceptionCommand, Unit>
{
    /// <inheritdoc />
    public Task<Unit> Handle(ThrowUnauthorizedExceptionCommand request, CancellationToken cancellationToken)
    {
        throw new UnauthorizedAccessException("You must be authenticated to access this resource.");
    }
}
