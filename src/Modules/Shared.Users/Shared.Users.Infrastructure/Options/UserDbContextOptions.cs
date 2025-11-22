using System.ComponentModel.DataAnnotations;
using Shared.Abstractions.Options;

namespace Shared.Users.Infrastructure.Options;

/// <summary>
/// Configuration options for the Users module DbContext.
/// Used to configure database connection and Entity Framework Core settings.
///
/// Configuration section in appsettings.json should contain:
/// - Users__Database__ConnectionString (required)
/// - Users__Database__EnableDetailedErrors (optional, default: false)
/// - Users__Database__EnableSensitiveDataLogging (optional, default: false)
///
/// Usage in module registration:
/// services.Configure&lt;DbContextOptions&gt;(
///     configuration.GetSection(DbContextOptions.SectionName));
/// </summary>
public class UserDbContextOptions : IOptions
{
    /// <summary>
    /// Configuration section name for binding this options class.
    /// Value: "Users__Database"
    /// </summary>
    public static string SectionName { get; } = "Users__Database";

    /// <summary>
    /// Gets or sets the PostgreSQL connection string for the Users database.
    /// Required. Example: "Host=localhost;Port=5432;Database=shared_users;Username=postgres;Password=postgres;"
    /// </summary>
    [Required(ErrorMessage = "ConnectionString is required for Users database")]
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether to enable detailed error messages in development.
    /// Helps with debugging but should not be used in production.
    /// Default: false
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to log sensitive data (like SQL parameters).
    /// WARNING: Only use in development, never in production as it can expose secrets.
    /// Default: false
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;
}
