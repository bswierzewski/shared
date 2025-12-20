using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Handler for ErrorResponseCommand that returns an error result.
/// </summary>
public class ErrorResponseCommandHandler : IRequestHandler<ErrorResponseCommand, ErrorOr<string>>
{
    /// <summary>
    /// Handles the ErrorResponseCommand by returning an error result.
    /// </summary>
    public Task<ErrorOr<string>> Handle(ErrorResponseCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult<ErrorOr<string>>(Error.Failure("test.error", "This is a test error response"));
    }
}
