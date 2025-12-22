namespace Shared.Abstractions.Authorization;

/// <summary>
/// A fluent builder for defining module permissions and roles.
/// </summary>
/// <remarks>
/// Initializes a new instance of the builder for a specific module.
/// </remarks>
/// <param name="module">The name of the module (e.g., "Users").</param>
public class AuthorizationBuilder(string module)
{
    private readonly List<Permission> _permissions = [];
    private readonly List<Role> _roles = [];

    /// <summary>
    /// Adds a permission definition to the internal collection.
    /// </summary>
    /// <param name="name">The unique permission identifier (e.g., "users.view").</param>
    /// <param name="displayName">The human-readable name.</param>
    /// <param name="description">Optional description.</param>
    public AuthorizationBuilder AddPermission(string name, string displayName, string description = "")
    {
        _permissions.Add(new Permission(name, displayName, module, description));
        return this;
    }

    /// <summary>
    /// Adds a role definition and assigns permissions by matching their names.
    /// </summary>
    /// <param name="name">The unique role identifier (e.g., "users.admin").</param>
    /// <param name="displayName">The human-readable name.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="permissionNames">A list of permission names to assign to this role.</param>
    public AuthorizationBuilder AddRole(string name, string displayName, string description, string[] permissionNames)
    {
        // Finds the permission objects that match the provided names.
        // Since there is no validation, if a name doesn't exist, it is simply ignored.
        var rolePermissions = _permissions
            .Where(p => permissionNames.Contains(p.Name))
            .ToList();

        _roles.Add(new Role(name, displayName, module, description, rolePermissions));
        return this;
    }

    /// <summary>
    /// Builds and returns the configured permissions and roles.
    /// </summary>
    public (IEnumerable<Permission> Permissions, IEnumerable<Role> Roles) Build()
    {
        return (_permissions, _roles);
    }
}
