using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Users.Application.Options;
using Shared.Users.Domain.Enums;

namespace Shared.Infrastructure.Tests.Authentication;

/// <summary>
/// Token provider for Supabase authentication.
/// Authenticates against Supabase REST API using email/password credentials.
/// Fetches access token from Supabase auth endpoint.
/// </summary>
public class SupabaseTokenProvider : ITokenProvider
{
    private readonly SupabaseOptions _supabaseOptions;
    private readonly TestUserOptions _userOptions;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Gets the identity provider type (Supabase).
    /// </summary>
    public IdentityProvider Provider => IdentityProvider.Supabase;

    /// <summary>
    /// Creates a new SupabaseTokenProvider from configuration options.
    /// </summary>
    /// <param name="supabaseOptions">Supabase configuration (authority and API key)</param>
    /// <param name="userOptions">Test user credentials (email and password)</param>
    /// <param name="httpClient">Optional custom HTTP client (for testing/mocking)</param>
    public SupabaseTokenProvider(
        IOptions<SupabaseOptions> supabaseOptions,
        IOptions<TestUserOptions> userOptions,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(supabaseOptions, nameof(supabaseOptions));
        ArgumentNullException.ThrowIfNull(userOptions, nameof(userOptions));

        _supabaseOptions = supabaseOptions.Value;
        _userOptions = userOptions.Value;
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Authenticates with Supabase using configured test user credentials.
    /// Parameters are ignored - uses email and password from configuration.
    /// </summary>
    /// <param name="login">Ignored - uses email from configuration</param>
    /// <param name="password">Ignored - uses password from configuration</param>
    /// <returns>Access token from Supabase auth endpoint</returns>
    /// <exception cref="InvalidOperationException">Thrown if authentication fails or token cannot be parsed</exception>
    public async Task<string> GetTokenAsync(string login, string password)
    {
        var tokenEndpoint = $"{_supabaseOptions.Authority.TrimEnd('/')}/auth/v1/token?grant_type=password";

        var request = new
        {
            email = _userOptions.Email,
            password = _userOptions.Password
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(request),
            System.Text.Encoding.UTF8,
            "application/json");

        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = content
            };
            httpRequest.Headers.Add("apikey", _supabaseOptions.ApiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            if (tokenResponse?.access_token is null)
            {
                throw new InvalidOperationException(
                    "No access_token in Supabase response. Check credentials and Supabase project configuration.");
            }

            return (string)tokenResponse.access_token;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to authenticate with Supabase at {_supabaseOptions.Authority}. Check authority URL and credentials.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse Supabase token response. Invalid JSON response from server.", ex);
        }
    }
}
