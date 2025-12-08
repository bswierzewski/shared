using MediatR;
using Shared.Infrastructure.Exceptions;

namespace Shared.Exceptions.Application.Commands.ThrowNotFoundException;

/// <summary>
/// Handler that throws a NotFoundException.
/// </summary>
public class ThrowNotFoundExceptionHandler : IRequestHandler<ThrowNotFoundExceptionCommand, Unit>
{
    /// <inheritdoc />
    public Task<Unit> Handle(ThrowNotFoundExceptionCommand request, CancellationToken cancellationToken)
    {
        throw new NotFoundException("Resource", "00000000-0000-0000-0000-000000000000");
    }
}
