using Shared.Abstractions.Abstractions;

namespace Shared.EnvFileGenerator.Tests.Unit.Options;

/// <summary>
/// Configuration options with nested objects.
/// Similar to IFirmaOptions structure.
/// </summary>
public class NestedOptions : IOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public static string SectionName => "NestedConfig";

    public string BaseUrl { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public NestedApiKeys Keys { get; set; } = new();
}

/// <summary>
/// Nested API keys configuration.
/// </summary>
public class NestedApiKeys
{
    public string? KeyA { get; set; } = "default-key-a";
    public string? KeyB { get; set; }
    public string? KeyC { get; set; } = "default-key-c";
}
