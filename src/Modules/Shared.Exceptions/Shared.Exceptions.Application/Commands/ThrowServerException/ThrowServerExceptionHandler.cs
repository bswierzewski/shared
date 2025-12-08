using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowServerException;

/// <summary>
/// Handler that throws an unexpected exception.
/// </summary>
public class ThrowServerExceptionHandler : IRequestHandler<ThrowServerExceptionCommand, Unit>
{
    /// <inheritdoc />
    public Task<Unit> Handle(ThrowServerExceptionCommand request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("An unexpected error occurred while processing the request.");
    }
}
