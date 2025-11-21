namespace Shared.Users.Domain.Entities;

/// <summary>
/// Role entity - represents an available role in the system.
/// Roles are system-wide entities that define sets of permissions.
/// </summary>
public class Role
{
    private readonly List<Permission> _permissions = new();
    private readonly List<Aggregates.User> _users = new();

    /// <summary>
    /// Unique role identifier
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Role name (e.g., "Admin", "Editor", "Viewer")
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description of the role
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Permissions assigned to this role (Many-to-Many relationship)
    /// </summary>
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// Users assigned this role (Many-to-Many relationship)
    /// </summary>
    public IReadOnlyCollection<Aggregates.User> Users => _users.AsReadOnly();

    /// <summary>
    /// When the role was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Whether the role is active (false = soft deleted)
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Whether this role is provided by a module (automatically registered)
    /// </summary>
    public bool IsModule { get; private set; } = false;

    /// <summary>
    /// The name of the module that provides this role (null if custom)
    /// </summary>
    public string? ModuleName { get; private set; }

    private Role() { }

    /// <summary>
    /// Marks this role as a module-provided role.
    /// </summary>
    /// <param name="moduleName">The name of the module that provides this role.</param>
    /// <param name="description">Optional description to update.</param>
    public void MarkAsModuleRole(string moduleName, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name cannot be empty", nameof(moduleName));

        IsModule = true;
        ModuleName = moduleName;

        if (description != null && Description != description)
        {
            Description = description;
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    public static Role Create(string name, string? description = null, bool isModule = false, string? moduleName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty", nameof(name));

        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true,
            IsModule = isModule,
            ModuleName = moduleName
        };
    }

    /// <summary>
    /// Add a permission to this role
    /// </summary>
    public void AddPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (!_permissions.Contains(permission))
            _permissions.Add(permission);
    }

    /// <summary>
    /// Remove a permission from this role
    /// </summary>
    public void RemovePermission(Permission permission)
    {
        if (permission != null)
            _permissions.Remove(permission);
    }

    /// <summary>
    /// Deactivate this role (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivate this role
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
}
