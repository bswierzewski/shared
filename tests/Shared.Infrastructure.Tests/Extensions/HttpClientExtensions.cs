using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Shared.Infrastructure.Tests.Extensions;

/// <summary>
/// Extensions for HttpClient to simplify common test operations.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Posts an object as JSON to the specified endpoint.
    /// </summary>
    /// <typeparam name="T">The type of data to serialize and post.</typeparam>
    /// <param name="client">The HTTP client instance.</param>
    /// <param name="endpoint">The API endpoint to post to.</param>
    /// <param name="data">The data to serialize as JSON and send.</param>
    /// <returns>The HTTP response from the server.</returns>
    public static async Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(endpoint, content);
    }

    /// <summary>
    /// Puts an object as JSON to the specified endpoint.
    /// </summary>
    /// <typeparam name="T">The type of data to serialize and put.</typeparam>
    /// <param name="client">The HTTP client instance.</param>
    /// <param name="endpoint">The API endpoint to put to.</param>
    /// <param name="data">The data to serialize as JSON and send.</param>
    /// <returns>The HTTP response from the server.</returns>
    public static async Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string endpoint, T data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PutAsync(endpoint, content);
    }

    /// <summary>
    /// Deserializes the response content as JSON to the specified type.
    /// Returns default value if content is empty or whitespace.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message.</param>
    /// <returns>The deserialized object or default value if content is empty.</returns>
    public static async Task<T?> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        // Return default if content is empty or whitespace
        if (string.IsNullOrWhiteSpace(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Adds a JWT Bearer token to the Authorization header of the HTTP client.
    /// </summary>
    /// <param name="client">The HTTP client instance.</param>
    /// <param name="token">The JWT token to add.</param>
    /// <returns>The HTTP client with the authorization header set.</returns>
    public static HttpClient WithBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Removes the Authorization header from the HTTP client.
    /// </summary>
    /// <param name="client">The HTTP client instance.</param>
    /// <returns>The HTTP client with the authorization header removed.</returns>
    public static HttpClient WithoutAuthorization(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }
}
