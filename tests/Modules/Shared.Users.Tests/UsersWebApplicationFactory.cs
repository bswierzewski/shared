using Microsoft.AspNetCore.Hosting;
using Shared.Infrastructure.Tests.Factories;

namespace Shared.Users.Tests;

/// <summary>
/// Test web application factory for the Users module.
/// Inherits from TestWebApplicationFactory to provide:
/// - PostgreSQL test container management (automatic startup/cleanup)
/// - Test database isolation with Respawn
/// - Database migration support
/// - Service customization hooks
/// </summary>
public class UsersWebApplicationFactory : TestWebApplicationFactory<TestHostProgram>
{
    /// <summary>
    /// Tables that should not be reset between tests.
    /// Roles and Permissions are system/module-defined data that should persist across tests.
    /// Only test data (users, assignments) are reset.
    /// </summary>
    protected override string[] TablesToIgnoreOnReset => ["Roles", "Permissions"];

    /// <summary>
    /// Configures the web host with the correct content root path.
    /// The test host needs to find appsettings files which are in the test project root.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set content root to a valid directory to avoid DirectoryNotFound exceptions
        // Use the directory of the test project assembly as a baseline
        var executingAssemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
        builder.UseContentRoot(executingAssemblyDirectory);

        // Call base implementation to apply other configurations
        base.ConfigureWebHost(builder);
    }
}
