using Shared.Abstractions.Options;

namespace Shared.EnvFileGenerator.Tests.Unit.Options;

/// <summary>
/// Simple configuration options without nested objects.
/// </summary>
public class SimpleOptions : IOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public static string SectionName => "SimpleConfig";

    public string ApiUrl { get; set; } = "https://api.example.com";
    public string ApiKey { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public bool EnableLogging { get; set; } = true;
    public decimal Timeout { get; set; } = 30.0m;
}
