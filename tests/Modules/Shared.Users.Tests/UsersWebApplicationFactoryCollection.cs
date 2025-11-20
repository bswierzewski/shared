using Xunit;

namespace Shared.Users.Tests;

/// <summary>
/// xUnit collection definition for sharing the UsersWebApplicationFactory across tests.
/// This allows all tests in this collection to share a single factory instance and database.
/// </summary>
[CollectionDefinition("Users Web Application Factory Collection")]
public class UsersWebApplicationFactoryCollection : ICollectionFixture<UsersWebApplicationFactory>
{
}
