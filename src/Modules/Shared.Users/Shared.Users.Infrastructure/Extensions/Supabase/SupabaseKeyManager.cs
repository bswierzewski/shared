using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols; // NuGet: Microsoft.IdentityModel.Protocols
using Shared.Users.Application.Options;

namespace Shared.Users.Infrastructure.Extensions.Supabase;

/// <summary>
/// Manages the retrieval and caching of Supabase JSON Web Key Sets (JWKS).
/// Uses Microsoft's <see cref="ConfigurationManager{T}"/> to handle thread safety, caching, and automatic refreshing.
/// </summary>
public class SupabaseKeyManager : IDisposable
{
    private readonly ConfigurationManager<JsonWebKeySet> _configManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupabaseKeyManager"/> class.
    /// </summary>
    /// <param name="options">Configuration options containing the Supabase Authority URL.</param>
    public SupabaseKeyManager(IOptions<SupabaseOptions> options)
    {
        var authority = options.Value.Authority
            ?? throw new ArgumentNullException(nameof(options.Value.Authority), "Supabase Authority URL must be provided.");
        var jwksUrl = $"{authority}/auth/v1/.well-known/jwks.json";

        // Initialize the ConfigurationManager with a custom retriever for JWKS
        _configManager = new ConfigurationManager<JsonWebKeySet>(
            jwksUrl,
            new JwksRetriever(),
            new HttpDocumentRetriever() // Standard HTTP retriever
        )
        {
            // AutomaticRefreshInterval: Controls how often the keys are automatically refreshed from the provider.
            // Supabase keys rarely change, so 24 hours is a safe default.
            AutomaticRefreshInterval = TimeSpan.FromHours(24),

            // RefreshInterval: The minimum time between immediate refreshes (to prevent DoS if the provider is down).
            RefreshInterval = TimeSpan.FromMinutes(5)
        };
    }

    /// <summary>
    /// Retrieves the valid signing keys.
    /// If the provided <paramref name="kid"/> is not found in the currently cached keys,
    /// a refresh is triggered to fetch the latest keys from Supabase.
    /// </summary>
    /// <param name="kid">The Key ID (kid) from the incoming JWT header.</param>
    /// <returns>A collection of valid <see cref="SecurityKey"/>s.</returns>
    public IEnumerable<SecurityKey> GetSigningKeys(string? kid)
    {
        // 1. Get the current configuration (fast, usually from memory)
        // .GetAwaiter().GetResult() is safe here as ConfigurationManager handles synchronization internally.
        var keySet = _configManager.GetConfigurationAsync().GetAwaiter().GetResult();

        // 2. Check if the incoming token's key ID exists in our cache
        if (!string.IsNullOrEmpty(kid) && !keySet.Keys.Any(k => k.KeyId == kid))
        {
            // 3. If the key is missing (e.g., Supabase rotated keys), request a refresh
            _configManager.RequestRefresh();

            // 4. Fetch the configuration again (this will trigger a network call if the RefreshInterval allows)
            keySet = _configManager.GetConfigurationAsync().GetAwaiter().GetResult();
        }

        return keySet.GetSigningKeys();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // ConfigurationManager does not strictly implement IDisposable,
        // but this method is kept for DI container compatibility if needed in the future.
    }

    /// <summary>
    /// A custom retriever adapter that parses the raw JSON response into a <see cref="JsonWebKeySet"/>.
    /// Required because Supabase does not provide a standard OpenID Connect discovery endpoint.
    /// </summary>
    private class JwksRetriever : IConfigurationRetriever<JsonWebKeySet>
    {
        public Task<JsonWebKeySet> GetConfigurationAsync(
            string address,
            IDocumentRetriever retriever,
            CancellationToken cancel)
        {
            return retriever.GetDocumentAsync(address, cancel)
                .ContinueWith(task => new JsonWebKeySet(task.Result), cancel);
        }
    }
}
