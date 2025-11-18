using Shared.Abstractions.Security;

namespace Shared.Abstractions.Modules;

/// <summary>
/// Contains metadata about a loaded module.
/// </summary>
/// <param name="Name">The unique name of the module.</param>
/// <param name="Permissions">Permissions defined by this module.</param>
/// <param name="Roles">Roles defined by this module.</param>
public sealed record ModuleInfo(
    string Name,
    IReadOnlyCollection<Permission> Permissions,
    IReadOnlyCollection<Role> Roles);
