using ErrorOr;
using MediatR;
using Shared.Abstractions.Authorization;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Command that is protected by requiring an "admin" role.
/// Used for testing authorization behavior when user doesn't have the required role.
/// Authorization is enforced via AuthorizeAttribute and validated in the MediatR pipeline.
/// </summary>
[Authorize(Roles = "admin")]
public record RoleProtectedCommand : IRequest<ErrorOr<string>>;
