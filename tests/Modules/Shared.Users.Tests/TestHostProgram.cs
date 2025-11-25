using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Modules;
using Shared.Users.Tests.Authentication;

namespace Shared.Users.Tests;

/// <summary>
/// Test host application for E2E testing of Users module.
/// Provides a minimal web application with test JWT authentication.
///
/// Each module is responsible for:
/// - Registering its own services in Register()
/// - Configuring its own middleware in Use()
/// </summary>
public class TestHostProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Discover modules (optionally exclude assemblies by prefix)
        var modules = ModuleLoader.LoadModules(
            exclusionPrefixes: null  // Add custom exclusions: new[] { "Legacy.", "Old." }
        );

        // Register all modules (each calls its own Register method)
        builder.Services.RegisterModules(modules, builder.Configuration);

        // Add test authentication
        builder.Services.AddAuthentication()
            .AddTestJwtBearer();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Middleware
        app.UseAuthentication();
        app.UseAuthorization();

        // Configure all modules (each calls its own Use method)
        app.UseModules(modules, builder.Configuration);

        app.Run();
    }
}

