using Shared.Abstractions.Abstractions;

namespace Shared.EnvFileGenerator.Tests.Unit.Options;

/// <summary>
/// Configuration options with multiple levels of nested objects.
/// </summary>
public class ComplexNestedOptions : IOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public static string SectionName => "ComplexConfig";

    public string ServiceName { get; set; } = string.Empty;
    public DatabaseConfig Database { get; set; } = new();
    public CacheConfig Cache { get; set; } = new();
}

/// <summary>
/// Database configuration with credentials nested inside.
/// </summary>
public class DatabaseConfig
{
    public string ConnectionString { get; set; } = "Server=localhost;Database=mydb";
    public int CommandTimeout { get; set; } = 30;
    public DatabaseCredentials Credentials { get; set; } = new();
}

/// <summary>
/// Database credentials.
/// </summary>
public class DatabaseCredentials
{
    public string? Username { get; set; } = "admin";
    public string? Password { get; set; }
}

/// <summary>
/// Cache configuration.
/// </summary>
public class CacheConfig
{
    public string Provider { get; set; } = "InMemory";
    public int ExpirationMinutes { get; set; } = 60;
    public CacheServerSettings ServerSettings { get; set; } = new();
}

/// <summary>
/// Cache server settings.
/// </summary>
public class CacheServerSettings
{
    public string? Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
}
