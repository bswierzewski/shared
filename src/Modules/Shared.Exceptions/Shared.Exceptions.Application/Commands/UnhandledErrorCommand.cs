using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Command that simulates an unhandled error by throwing an exception.
/// Used for testing error handling behavior.
/// </summary>
public record UnhandledErrorCommand : IRequest<ErrorOr<string>>;
