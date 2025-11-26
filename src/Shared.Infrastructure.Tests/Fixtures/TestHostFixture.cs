using Shared.Infrastructure.Tests.Builders;
using Shared.Infrastructure.Tests.Core;

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
///     public TestContext Context { get; private set; } = null!;
///
///     public async Task InitializeAsync()
///     {
///         Context = await TestContext.CreateBuilder&lt;TestHostProgram&gt;()
///             .WithPostgreSql()
///             .WithTablesIgnoredOnReset("Roles", "Permissions")
///             .BuildAsync();
///     }
///
///     public Task DisposeAsync() => Context.DisposeAsync().AsTask();
/// }
///
/// // Define a collection
/// [CollectionDefinition("Users")]
/// public class UsersCollection : ICollectionFixture&lt;UsersTestFixture&gt; { }
///
/// // Use in tests
/// [Collection("Users")]
/// public class UserEndpointsTests
/// {
///     private readonly TestContext _context;
///
///     public UserEndpointsTests(UsersTestFixture fixture)
///     {
///         _context = fixture.Context;
///     }
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
    /// Initializes the fixture by building the test context.
    /// Called once before all tests in the collection.
    /// </summary>
    public async Task InitializeAsync()
    {
        var builder = TestContext.CreateBuilder<TProgram>();
        builder = _configure(builder);
        Context = await builder.BuildAsync();
    }

    /// <summary>
    /// Disposes the fixture by disposing the test context.
    /// Called once after all tests in the collection complete.
    /// </summary>
    public Task DisposeAsync() => Context.DisposeAsync().AsTask();
}

/// <summary>
/// Base fixture with PostgreSQL for common scenarios.
/// Provides a pre-configured PostgreSQL test container.
/// </summary>
/// <typeparam name="TProgram">The Program or Startup class of the application under test.</typeparam>
/// <example>
/// <code>
/// public class MyTestFixture : PostgreSqlTestHostFixture&lt;Program&gt;
/// {
///     public MyTestFixture() : base(builder => builder
///         .WithTablesIgnoredOnReset("Roles", "Permissions"))
///     {
///     }
/// }
/// </code>
/// </example>
public class PostgreSqlTestHostFixture<TProgram> : TestHostFixture<TProgram> where TProgram : class
{
    /// <summary>
    /// Initializes a new instance with PostgreSQL test container.
    /// </summary>
    /// <param name="configure">Optional additional configuration for the test context builder.</param>
    public PostgreSqlTestHostFixture(
        Func<TestContextBuilder<TProgram>, TestContextBuilder<TProgram>>? configure = null)
        : base(builder =>
        {
            builder = builder.WithPostgreSql();
            return configure?.Invoke(builder) ?? builder;
        })
    {
    }
}
