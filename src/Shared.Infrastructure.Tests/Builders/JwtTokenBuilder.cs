using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Infrastructure.Tests.Builders;

/// <summary>
/// Utility for building test JWT tokens with custom claims.
/// Uses JwtSecurityTokenHandler to create valid, parseable JWT tokens.
/// Signature validation is disabled in test environment, so any signing key works.
/// </summary>
public class JwtTokenBuilder
{
    private readonly Dictionary<string, object> _customClaims = new();
    private DateTime _expiry = DateTime.UtcNow.AddHours(1);
    private string? _subject;
    private string? _email;

    /// <summary>
    /// Sets the subject (sub) claim - typically the external user ID from auth provider.
    /// </summary>
    public JwtTokenBuilder WithSubject(string sub)
    {
        _subject = sub;
        return this;
    }

    /// <summary>
    /// Sets the email claim.
    /// </summary>
    public JwtTokenBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the display name claim (mapped to ClaimTypes.Name in production).
    /// </summary>
    public JwtTokenBuilder WithDisplayName(string name)
    {
        _customClaims["name"] = name;
        return this;
    }

    /// <summary>
    /// Adds a custom claim to the token.
    /// </summary>
    public JwtTokenBuilder WithClaim(string key, string value)
    {
        _customClaims[key] = value;
        return this;
    }

    /// <summary>
    /// Sets the token expiration time (exp claim).
    /// Default is 1 hour from now.
    /// </summary>
    public JwtTokenBuilder WithExpiration(DateTime expiry)
    {
        _expiry = expiry;
        return this;
    }

    /// <summary>
    /// Builds the JWT token using JwtSecurityTokenHandler.
    /// Uses a test signing key (validation is disabled in test environment anyway).
    /// </summary>
    /// <returns>A valid JWT token string that can be parsed by JwtSecurityTokenHandler</returns>
    public string Build()
    {
        // Create claims from builder properties and custom claims
        // Use standard JWT claim names ("sub", "email") instead of .NET claim type URIs
        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(_subject))
            claims.Add(new Claim("sub", _subject));

        if (!string.IsNullOrEmpty(_email))
            claims.Add(new Claim("email", _email));

        // Add custom claims
        foreach (var customClaim in _customClaims)
        {
            claims.Add(new Claim(customClaim.Key, customClaim.Value.ToString() ?? ""));
        }

        // Create signing credentials (uses dummy key since validation is disabled)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-test-secret-test-secret-test-secret"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create JWT token
        var token = new JwtSecurityToken(
            issuer: "test",
            audience: "test",
            claims: claims,
            expires: _expiry,
            signingCredentials: credentials
        );

        // Write token to string
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}
