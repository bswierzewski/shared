using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowNotFoundException;

/// <summary>
/// Command that throws a NotFoundException.
/// Used for testing 404 error handling in development environments.
/// </summary>
public record ThrowNotFoundExceptionCommand : IRequest<Unit>;
