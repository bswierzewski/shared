using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Modules;
using Shared.Users.Tests.Authentication;

namespace Shared.Users.Tests;

/// <summary>
/// Test host application for E2E testing of Users module.
/// Provides a minimal web application with test JWT authentication.
/// Uses ModuleLoader to automatically discover and register all modules.
/// This class is used by WebApplicationFactory<TestHostProgram> as the entry point.
/// </summary>
public class TestHostProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register all modules (discovers them automatically via reflection)
        builder.Services.AddModules(builder.Configuration);

        // Add test JWT Bearer authentication (overrides production Supabase/Clerk)
        builder.Services.AddAuthentication()
            .AddTestJwtBearer();

        // Add authorization
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Authentication middleware
        app.UseAuthentication();

        // Authorization middleware
        app.UseAuthorization();

        // Configure all modules' middleware (includes JIT provisioning, endpoints mapping, etc.)
        var configuration = app.Services.GetRequiredService<IConfiguration>();
        app.UseModules(configuration);

        app.Run();
    }
}

