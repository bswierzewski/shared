using ErrorOr;
using MediatR;
using Shared.Abstractions.Authorization;
using static Shared.Exceptions.Application.ModuleConstants;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Command that is protected by requiring an "admin" role.
/// Used for testing authorization behavior when user doesn't have the required role.
/// Authorization is enforced via AuthorizeAttribute and validated in the MediatR pipeline.
/// </summary>
[Authorize(Roles = Roles.Tester)] // Note: Using Users module's admin role
public record RoleProtectedCommand : IRequest<ErrorOr<string>>;
