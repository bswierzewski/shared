namespace Shared.Users.Domain.Entities;

/// <summary>
/// Role entity - represents an available role in the system.
/// Roles are system-wide entities that define sets of permissions.
/// </summary>
public class Role
{
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
    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    /// <summary>
    /// Users assigned this role (Many-to-Many relationship)
    /// </summary>
    public ICollection<Aggregates.User> Users { get; private set; } = new List<Aggregates.User>();

    /// <summary>
    /// When the role was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Whether the role is active (false = soft deleted)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this role is provided by a module (automatically registered)
    /// </summary>
    public bool IsModule { get; set; } = false;

    /// <summary>
    /// The name of the module that provides this role (null if custom)
    /// </summary>
    public string? ModuleName { get; set; }

    private Role() { }

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

        if (!Permissions.Contains(permission))
            Permissions.Add(permission);
    }

    /// <summary>
    /// Remove a permission from this role
    /// </summary>
    public void RemovePermission(Permission permission)
    {
        if (permission != null)
            Permissions.Remove(permission);
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
