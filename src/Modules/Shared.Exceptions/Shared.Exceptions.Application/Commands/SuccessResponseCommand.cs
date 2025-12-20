using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Command that returns a successful response.
/// Used for testing successful error handling behavior.
/// </summary>
public record SuccessResponseCommand : IRequest<ErrorOr<string>>;
