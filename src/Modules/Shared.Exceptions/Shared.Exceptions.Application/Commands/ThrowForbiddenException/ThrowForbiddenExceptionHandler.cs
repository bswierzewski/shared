using MediatR;
using Shared.Infrastructure.Exceptions;

namespace Shared.Exceptions.Application.Commands.ThrowForbiddenException;

/// <summary>
/// Handler that throws a ForbiddenAccessException.
/// </summary>
public class ThrowForbiddenExceptionHandler : IRequestHandler<ThrowForbiddenExceptionCommand, Unit>
{
    /// <inheritdoc />
    public Task<Unit> Handle(ThrowForbiddenExceptionCommand request, CancellationToken cancellationToken)
    {
        throw new ForbiddenAccessException("You do not have permission to access this resource.");
    }
}
