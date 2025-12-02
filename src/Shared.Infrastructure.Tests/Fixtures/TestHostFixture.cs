using Shared.Infrastructure.Tests.Builders;
using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Infrastructure.Containers;

namespace Shared.Infrastructure.Tests.Fixtures;

/// <summary>
/// Base xUnit fixture for sharing test infrastructure across multiple test classes.
/// Use with [CollectionFixture] for performance optimization by reusing containers and hosts.
/// </summary>
/// <typeparam name="TProgram">The Program or Startup class of the application under test.</typeparam>
/// <example>
/// <code>
/// // Define a fixture for your module
/// public class UsersTestFixture : IAsyncLifetime
/// {
///     public PostgreSqlTestContainer Container { get; } = new();
///     public TestContext Context { get; private set; } = null!;
///
///     public async Task InitializeAsync()
///     {
///         await Container.StartAsync();
///         Context = await TestContext.CreateBuilder&lt;Program&gt;()
///             .WithContainer(Container)
///             .WithTablesIgnoredOnReset("Roles", "Permissions")
///             .BuildAsync();
///     }
///
///     public async Task DisposeAsync()
///     {
///         await Context.DisposeAsync();
///         await Container.StopAsync();
///     }
/// }
///
/// // Define a collection
/// [CollectionDefinition("Users")]
/// public class UsersCollection : ICollectionFixture&lt;UsersTestFixture&gt; { }
///
/// // Use in tests
/// [Collection("Users")]
/// public class UserEndpointsTests(UsersTestFixture fixture)
/// {
///     private readonly TestContext _context = fixture.Context;
///
///     [Fact]
///     public async Task GetUser_ReturnsUser()
///     {
///         await _context.ResetDatabaseAsync();
///         // Test implementation...
///     }
/// }
/// </code>
/// </example>
public class TestHostFixture<TProgram> : IAsyncLifetime, ITestHostFixture where TProgram : class
{
    private readonly Func<TestContextBuilder<TProgram>, TestContextBuilder<TProgram>> _configure;

    /// <summary>
    /// Gets the shared PostgreSQL container.
    /// </summary>
    public PostgreSqlTestContainer Container { get; } = new();

    /// <summary>
    /// Gets the shared test context.
    /// </summary>
    public TestContext Context { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the test host fixture.
    /// </summary>
    /// <param name="configure">Optional configuration for the test context builder.</param>
    public TestHostFixture(
        Func<TestContextBuilder<TProgram>, TestContextBuilder<TProgram>>? configure = null)
    {
        _configure = configure ?? (builder => builder);
    }

    /// <summary>
    /// Initializes the fixture by starting the container and building the test context.
    /// Called once before all tests in the collection.
    /// </summary>
    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var builder = TestContext.CreateBuilder<TProgram>()
            .WithContainer(Container);
        builder = _configure(builder);
        Context = await builder.BuildAsync();
    }

    /// <summary>
    /// Disposes the fixture by disposing the test context and stopping the container.
    /// Called once after all tests in the collection complete.
    /// </summary>
    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }

        await Container.StopAsync();
    }
}
