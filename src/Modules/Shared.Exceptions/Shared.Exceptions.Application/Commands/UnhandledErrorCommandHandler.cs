using ErrorOr;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Handler for UnhandledErrorCommand that throws an unhandled exception.
/// Used to test how the application handles unexpected errors.
/// </summary>
public class UnhandledErrorCommandHandler : IRequestHandler<UnhandledErrorCommand, ErrorOr<string>>
{
    /// <summary>
    /// Handles the UnhandledErrorCommand by throwing an exception.
    /// </summary>
    public Task<ErrorOr<string>> Handle(UnhandledErrorCommand request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("This is an unhandled error for testing purposes");
    }
}
