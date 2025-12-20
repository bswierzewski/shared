using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Handler for SuccessResponseCommand that returns a successful result.
/// </summary>
public class SuccessResponseCommandHandler : IRequestHandler<SuccessResponseCommand, ErrorOr<string>>
{
    /// <summary>
    /// Handles the SuccessResponseCommand by returning a successful response.
    /// </summary>
    public Task<ErrorOr<string>> Handle(SuccessResponseCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult<ErrorOr<string>>("Success response");
    }
}
