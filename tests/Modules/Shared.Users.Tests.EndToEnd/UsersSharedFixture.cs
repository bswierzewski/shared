using Shared.Infrastructure.Tests.Authentication;
using Shared.Infrastructure.Tests.Infrastructure.Containers;

namespace Shared.Users.Tests;

/// <summary>
/// Shared test fixture for Users module integration tests.
/// Provides shared infrastructure (PostgreSQL container, TestContext) across all test classes.
/// </summary>
public class UsersSharedFixture : IAsyncLifetime
{
    /// <summary>
    /// Gets the shared PostgreSQL container.
    /// </summary>
    public PostgreSqlTestContainer Container { get; } = new();

    /// <summary>
    /// Gets the token provider for authentication.
    /// Shared across all tests - has built-in caching.
    /// </summary>
    public ITokenProvider TokenProvider { get; private set; } = null!;

    /// <summary>
    /// Gets the test user options (email, password) from configuration.
    /// </summary>
    public TestUserOptions TestUser { get; private set; } = null!;

    /// <summary>
    /// Required for interface - no-op.
    /// </summary>
    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Required for interface - no-op.
    /// </summary>
    public Task DisposeAsync() => Task.CompletedTask;
}

/// <summary>
/// xUnit collection definition for sharing the UsersTestFixture across tests.
/// All tests with [Collection("Users")] share a single PostgreSQL container and TestContext.
/// </summary>
[CollectionDefinition("Users")]
public class UsersCollection : ICollectionFixture<UsersSharedFixture>
{
}
