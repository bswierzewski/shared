using ErrorOr;
using MediatR;
using Shared.Abstractions.Authorization;
using static Shared.Exceptions.Application.Module;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Command that is protected by requiring an "admin" role.
/// Used for testing authorization behavior when user doesn't have the required role.
/// Authorization is enforced via AuthorizeAttribute and validated in the MediatR pipeline.
/// </summary>
[Authorize(Permissions = [Permissions.View, Permissions.Create])]
public record AuthorizeCommand : IRequest<ErrorOr<string>>;
