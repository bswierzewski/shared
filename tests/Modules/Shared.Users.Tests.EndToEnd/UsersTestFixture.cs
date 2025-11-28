using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Tests.Authentication;
using Shared.Infrastructure.Tests.Core;

namespace Shared.Users.Tests;

/// <summary>
/// Test fixture for Users module integration tests.
/// Provides shared test infrastructure with PostgreSQL container and test authentication.
/// Uses the Shared.Users.Web project as the test host.
/// </summary>
public class UsersTestFixture : IAsyncLifetime
{
    public TestContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Context = await TestContext.CreateBuilder<Program>()
            .WithPostgreSql()
            .WithTablesIgnoredOnReset("Roles", "Permissions") // Preserve system/reference data
            .WithServices((services, configuration) =>
            {
                // Register test user credentials from appsettings
                services.ConfigureOptions<TestUserOptions>(configuration);

                // Register Supabase token provider for authentication
                services.AddSingleton<ITokenProvider, SupabaseTokenProvider>();
            })
            .BuildAsync();
    }

    public Task DisposeAsync() => Context?.DisposeAsync().AsTask() ?? Task.CompletedTask;
}
