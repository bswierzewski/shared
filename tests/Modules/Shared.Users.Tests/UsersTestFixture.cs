using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Tests.Builders;
using Shared.Infrastructure.Tests.Core;
using Shared.Users.Infrastructure.Persistence;
using Shared.Users.Tests.Authentication;

namespace Shared.Users.Tests;

/// <summary>
/// Test fixture for Users module integration tests.
/// Provides shared test infrastructure with PostgreSQL container and test authentication.
/// </summary>
public class UsersTestFixture : IAsyncLifetime
{
    public TestContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Context = await TestContext.CreateBuilder<Program>()
            .WithPostgreSql()
            .WithConnectionStringKeys(
                OptionsExtensions.For<UsersDbContextOptions>(x => x.ConnectionString)
            )
            .WithTablesIgnoredOnReset("Roles", "Permissions") // Preserve system/reference data
            .WithHostConfiguration(builder =>
            {
                // Set content root to find appsettings files
                var executingAssemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
                builder.UseContentRoot(executingAssemblyDirectory);
            })
            .WithServices(services =>
            {
                // Configure test authentication
                services.AddAuthentication().AddTestJwtBearer();
                services.AddAuthorization();
            })
            .BuildAsync();
    }

    public Task DisposeAsync() => Context?.DisposeAsync().AsTask() ?? Task.CompletedTask;
}
