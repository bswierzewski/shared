using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Command that returns an error response.
/// Used for testing error handling behavior with ErrorOr pattern.
/// </summary>
public record ErrorResponseCommand : IRequest<ErrorOr<string>>;
