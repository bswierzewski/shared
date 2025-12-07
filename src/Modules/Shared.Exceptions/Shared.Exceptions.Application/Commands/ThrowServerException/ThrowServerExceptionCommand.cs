using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowServerException;

/// <summary>
/// Command that throws an unexpected exception.
/// Used for testing 500 error handling in development environments.
/// </summary>
public record ThrowServerExceptionCommand : IRequest<Unit>;
