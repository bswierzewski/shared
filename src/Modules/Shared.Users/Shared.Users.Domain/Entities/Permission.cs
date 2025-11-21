namespace Shared.Users.Domain.Entities;

/// <summary>
/// Permission entity - represents an available permission in the system.
/// Permissions are system-wide entities that define granular access controls.
/// </summary>
public class Permission
{
    private readonly List<Role> _roles = new();
    private readonly List<Aggregates.User> _users = new();

    /// <summary>
    /// Unique permission identifier
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Permission name (e.g., "users.view", "users.edit", "products.delete")
    /// Uses dot notation for hierarchical organization
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional description of what this permission allows
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Roles that have this permission (Many-to-Many relationship)
    /// </summary>
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

    /// <summary>
    /// Users that have this permission directly granted (Many-to-Many relationship)
    /// </summary>
    public IReadOnlyCollection<Aggregates.User> Users => _users.AsReadOnly();

    /// <summary>
    /// When the permission was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Whether the permission is active (false = soft deleted)
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Whether this permission is provided by a module (automatically registered)
    /// </summary>
    public bool IsModule { get; private set; } = false;

    /// <summary>
    /// The name of the module that provides this permission (null if custom)
    /// </summary>
    public string? ModuleName { get; private set; }

    /// <summary>
    /// Marks this permission as a module-provided permission.
    /// </summary>
    /// <param name="moduleName">The name of the module that provides this permission.</param>
    /// <param name="description">Optional description to update.</param>
    public void MarkAsModulePermission(string moduleName, string? description = null)
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

    private Permission() { }

    /// <summary>
    /// Create a new permission
    /// </summary>
    public static Permission Create(string name, string? description = null, bool isModule = false, string? moduleName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty", nameof(name));

        return new Permission
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
    /// Deactivate this permission (soft delete)
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivate this permission
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
}
