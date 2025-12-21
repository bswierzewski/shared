using Shared.Abstractions.Primitives;
using Shared.Users.Domain.DomainEvents;
using Shared.Users.Domain.Entities;
using Shared.Users.Domain.Enums;

namespace Shared.Users.Domain.Aggregates;

/// <summary>
/// User aggregate root - manages user data, roles, and external provider mappings.
/// Aggregate root in Domain-Driven Design: the only entity that can be referenced from outside the aggregate.
///
/// Responsibilities:
/// - Manage user profile (email, isActive)
/// - Track last login timestamp
/// - Manage external provider mappings (multiple providers for same user email)
/// - Manage role assignments
/// Note: Display name comes from JWT token and is not persisted - it's metadata from the provider
/// </summary>
public class User : AggregateRoot<Guid>
{
    private readonly List<ExternalProvider> _externalProviders = new();
    private readonly List<Role> _roles = new();

    /// <summary>
    /// User's email address (unique identifier for JIT provisioning)
    /// </summary>
    public string Email { get; private set; } = null!;

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
    public IReadOnlyCollection<ExternalProvider> ExternalProviders => _externalProviders.AsReadOnly();

    /// <summary>
    /// Assigned roles (Many-to-Many relationship with Role table)
    /// User has a role = row exists in User_Role table
    /// </summary>
    public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();

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
    /// <param name="provider">The identity provider used for provisioning</param>
    /// <param name="externalUserId">The external user ID from the provider</param>
    /// <returns>A new User aggregate with the external provider linked</returns>
    public static User ProvisionNew(
        string email,
        IdentityProvider provider,
        string externalUserId)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            IsActive = true,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        // Link the external provider
        user._externalProviders.Add(ExternalProvider.Create(user.Id, provider, externalUserId));

        // Emit domain event
        user.AddDomainEvent(new UserProvisionedEvent(user.Id, email, provider));

        return user;
    }

    /// <summary>
    /// Update user's last login timestamp
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Link an additional external provider to this user (email-based linking)
    /// Used when a user authenticates with a different provider but same email
    /// </summary>
    /// <param name="provider">The identity provider to link</param>
    /// <param name="externalUserId">The external user ID from the provider</param>
    /// <returns>The newly created ExternalProvider if this is a new provider link, null if it already existed</returns>
    public ExternalProvider? LinkExternalProvider(IdentityProvider provider, string externalUserId)
    {
        // Check if this specific provider+externalUserId combination is already linked
        var existing = _externalProviders.FirstOrDefault(ep => ep.Provider == provider && ep.ExternalUserId == externalUserId);
        if (existing != null)
        {
            // This provider ID is already linked, just update last login
            UpdateLastLogin();
            return null;
        }

        // Link new provider ID (allows multiple IDs from same provider)
        var newProvider = ExternalProvider.Create(Id, provider, externalUserId);
        _externalProviders.Add(newProvider);

        // Emit domain event
        AddDomainEvent(new ExternalProviderLinkedEvent(Id, provider));

        return newProvider;
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
        if (_roles.Any(r => r.Name == role.Name))
            return;

        _roles.Add(role);

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

        if (_roles.Remove(role))
        {
            // Emit domain event only if role was actually removed
            AddDomainEvent(new UserRoleAssignedEvent(Id, role.Name)); // Or create RemoveRoleEvent
        }
    }
}
