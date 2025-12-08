using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowUnauthorizedException;

/// <summary>
/// Command that throws an UnauthorizedAccessException.
/// Used for testing 401 error handling in development environments.
/// </summary>
public record ThrowUnauthorizedExceptionCommand : IRequest<Unit>;
