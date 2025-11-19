using Shared.Abstractions.Primitives;
using Shared.Users.Domain.DomainEvents;
using Shared.Users.Domain.Entities;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Domain.Aggregates;

/// <summary>
/// User aggregate root - manages user data, roles, permissions, and external provider mappings.
/// Aggregate root in Domain-Driven Design: the only entity that can be referenced from outside the aggregate.
///
/// Responsibilities:
/// - Manage user profile (email, displayName, pictureUrl, isActive)
/// - Track last login timestamp
/// - Manage external provider mappings (multiple providers for same user email)
/// - Manage role assignments
/// - Manage direct permission grants
/// </summary>
public class User : AggregateRoot<Guid>
{
    /// <summary>
    /// User's email address (unique identifier for JIT provisioning)
    /// </summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// User's display name (from external provider or manually set)
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// User's profile picture URL (from external provider or manually set)
    /// </summary>
    public string? PictureUrl { get; private set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// When the user last logged in (null if never logged in)
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; private set; }

    /// <summary>
    /// External provider mappings (one user can have multiple external providers)
    /// Navigation property for One-to-Many relationship with ExternalProvider table
    /// </summary>
    public ICollection<ExternalProvider> ExternalProviders { get; private set; } = new List<ExternalProvider>();

    /// <summary>
    /// Assigned roles (Many-to-Many relationship with Role table)
    /// User has a role = row exists in User_Role table
    /// </summary>
    public ICollection<Role> Roles { get; private set; } = new List<Role>();

    /// <summary>
    /// Directly granted permissions (Many-to-Many relationship with Permission table)
    /// User has a permission = row exists in User_Permission table
    /// </summary>
    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    /// <summary>
    /// Private constructor for EF Core only
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// Factory method to provision a new user during JIT flow
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="displayName">User's display name from external provider</param>
    /// <param name="pictureUrl">User's profile picture URL from external provider</param>
    /// <param name="provider">The identity provider used for provisioning</param>
    /// <param name="externalUserId">The external user ID from the provider</param>
    /// <returns>A new User aggregate with the external provider linked</returns>
    public static User ProvisionNew(
        string email,
        string? displayName,
        string? pictureUrl,
        IdentityProvider provider,
        string externalUserId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            PictureUrl = pictureUrl,
            IsActive = true,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        // Link the external provider
        user.ExternalProviders.Add(ExternalProvider.Create(user.Id, provider, externalUserId));

        // Emit domain event
        user.AddDomainEvent(new UserProvisionedEvent(user.Id, email, provider));

        return user;
    }

    /// <summary>
    /// Update user's profile information
    /// </summary>
    /// <param name="displayName">New display name (null to keep existing)</param>
    /// <param name="pictureUrl">New picture URL (null to keep existing)</param>
    public void UpdateProfile(string? displayName, string? pictureUrl)
    {
        if (!string.IsNullOrEmpty(displayName))
            DisplayName = displayName;

        if (!string.IsNullOrEmpty(pictureUrl))
            PictureUrl = pictureUrl;

        LastLoginAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Link an additional external provider to this user (email-based linking)
    /// Used when a user authenticates with a different provider but same email
    /// </summary>
    /// <param name="provider">The identity provider to link</param>
    /// <param name="externalUserId">The external user ID from the provider</param>
    public void LinkExternalProvider(IdentityProvider provider, string externalUserId)
    {
        // Check if this provider is already linked
        var existing = ExternalProviders.FirstOrDefault(ep => ep.Provider == provider);
        if (existing != null)
        {
            // Provider already linked, just update last login
            LastLoginAt = DateTimeOffset.UtcNow;
            return;
        }

        // Link new provider
        ExternalProviders.Add(ExternalProvider.Create(Id, provider, externalUserId));

        // Emit domain event
        AddDomainEvent(new ExternalProviderLinkedEvent(Id, provider));
    }

    /// <summary>
    /// Assign a role to this user
    /// </summary>
    /// <param name="role">The role to assign</param>
    public void AssignRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        // Check if user already has this role
        if (Roles.Any(r => r.Id == role.Id))
            return;

        Roles.Add(role);

        // Emit domain event
        AddDomainEvent(new UserRoleAssignedEvent(Id, role.Name));
    }

    /// <summary>
    /// Remove a role from this user
    /// </summary>
    /// <param name="role">The role to remove</param>
    public void RemoveRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (Roles.Remove(role))
        {
            // Emit domain event only if role was actually removed
            AddDomainEvent(new UserRoleAssignedEvent(Id, role.Name)); // Or create RemoveRoleEvent
        }
    }

    /// <summary>
    /// Grant a direct permission to this user
    /// </summary>
    /// <param name="permission">The permission to grant</param>
    public void GrantPermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        // Check if user already has this permission
        if (Permissions.Any(p => p.Id == permission.Id))
            return;

        Permissions.Add(permission);

        // Emit domain event
        AddDomainEvent(new UserPermissionGrantedEvent(Id, permission.Name));
    }

    /// <summary>
    /// Revoke a direct permission from this user
    /// </summary>
    /// <param name="permission">The permission to revoke</param>
    public void RevokePermission(Permission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        if (Permissions.Remove(permission))
        {
            // Emit domain event only if permission was actually removed
            AddDomainEvent(new UserPermissionGrantedEvent(Id, permission.Name)); // Or create RevokePermissionEvent
        }
    }
}
