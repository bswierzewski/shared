namespace Shared.Users.Tests;

/// <summary>
/// xUnit collection definition for sharing the UsersTestFixture across tests.
/// This allows all tests in this collection to share a single test context, container, and database.
/// </summary>
[CollectionDefinition("Users")]
public class UsersCollection : ICollectionFixture<UsersTestFixture>
{
}
