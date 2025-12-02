using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Tests.Authentication;
using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Infrastructure.Containers;

namespace Shared.Users.Tests;

/// <summary>
/// Shared test fixture for Users module integration tests.
/// Provides shared infrastructure (PostgreSQL container, TestContext) across all test classes.
/// </summary>
/// <remarks>
/// This fixture is created ONCE per test collection and shared across all test classes.
/// Since Users module tests don't use mocks, they share a single TestContext.
/// 
/// It provides:
/// - PostgreSQL container (started once, shared)
/// - TestContext (shared across all tests)
/// - Token provider with built-in cache
/// </remarks>
public class UsersTestFixture : IAsyncLifetime
{
    /// <summary>
    /// Gets the shared PostgreSQL container.
    /// </summary>
    public PostgreSqlTestContainer Container { get; } = new();

    /// <summary>
    /// Gets the shared test context.
    /// All tests in the collection use this same context.
    /// </summary>
    public TestContext Context { get; private set; } = null!;

    /// <summary>
    /// Gets the test user options (email, password) from configuration.
    /// </summary>
    public TestUserOptions TestUser { get; private set; } = null!;

    /// <summary>
    /// Gets the token provider for authentication.
    /// Shared across all tests - has built-in caching.
    /// </summary>
    public ITokenProvider TokenProvider { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Start PostgreSQL container (once for all tests)
        await Container.StartAsync();

        // Create shared test context
        Context = await TestContext.CreateBuilder<Program>()
            .WithContainer(Container)
            .WithServices((services, configuration) =>
            {
                // Register test user credentials from appsettings
                services.ConfigureOptions<TestUserOptions>(configuration);

                // Register Supabase token provider for authentication
                services.AddSingleton<ITokenProvider, SupabaseTokenProvider>();
            })
            .BuildAsync();

        // Get test user configuration and token provider
        TestUser = Context.GetRequiredService<IOptions<TestUserOptions>>().Value;
        TokenProvider = Context.GetRequiredService<ITokenProvider>();
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }

        await Container.StopAsync();
    }
}

/// <summary>
/// xUnit collection definition for sharing the UsersTestFixture across tests.
/// All tests with [Collection("Users")] share a single PostgreSQL container and TestContext.
/// </summary>
[CollectionDefinition("Users")]
public class UsersCollection : ICollectionFixture<UsersTestFixture>
{
}
