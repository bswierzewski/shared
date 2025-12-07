using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowForbiddenException;

/// <summary>
/// Command that throws a ForbiddenAccessException.
/// Used for testing 403 error handling in development environments.
/// </summary>
public record ThrowForbiddenExceptionCommand : IRequest<Unit>;
