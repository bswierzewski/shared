using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Tests.Authentication;
using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Extensions.Http;
using Shared.Users.Application.DTOs;
using Shared.Users.Infrastructure.Persistence;

namespace Shared.Users.Tests.EndToEnd.UseCase;

/// <summary>
/// E2E tests for Users module HTTP endpoints.
/// Tests user retrieval, role management, and permission management endpoints.
/// </summary>
[Collection("Users")]
public class UserEndpointsTests : IAsyncLifetime
{
    private readonly TestContext _context;
    private TestUserOptions _testUser = null!;

    public UserEndpointsTests(UsersTestFixture fixture)
    {
        _context = fixture.Context;
    }

    public async Task InitializeAsync()
    {
        _testUser = _context.Services.GetRequiredService<IOptions<TestUserOptions>>().Value;
        await Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Tests the GET /api/users/{userId} endpoint retrieves an existing user.
    /// </summary>
    [Fact]
    public async Task GetUserById_WithValidUser_ShouldReturnUser()
    {
        // Arrange - Reset database and provision a user
        await _context.ResetDatabaseAsync();

        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        // Provision user
        await _context.Client.GetAsync("/api/users");

        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Act - Get user by ID
        var response = await _context.Client.GetAsync($"/api/users/{user.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.ReadAsJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Id.Should().Be(user.Id);
        userDto.Email.Should().Be(_testUser.Email);
    }

    /// <summary>
    /// Tests the GET /api/users/{userId} endpoint returns 404 for non-existent user.
    /// </summary>
    [Fact]
    public async Task GetUserById_NonExistent_ShouldReturn404()
    {
        // Arrange
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        // Act
        var response = await _context.Client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests the POST /api/users/{userId}/roles/{roleName} endpoint assigns a role to user.
    /// </summary>
    [Fact]
    public async Task AssignRoleToUser_ShouldSucceed()
    {
        // Arrange - Provision user and get their ID
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        // Provision the user
        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Act - Assign role
        var response = await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/roles/admin",
            new { });

        // Assert - Endpoint returns success
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role was assigned in database
        var userWithRoles = await _context.GetUserWithRolesFromDbAsync(_testUser.Email);
        userWithRoles.Roles.Should().Contain(r => r.Name == "admin");
    }

    /// <summary>
    /// Tests that assigning the same role twice is idempotent (doesn't fail).
    /// </summary>
    [Fact]
    public async Task AssignRoleToUser_Idempotent_SecondAssignmentShouldSucceed()
    {
        // Arrange
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Act - Assign role first time
        var response1 = await _context.Client.PostJsonAsync($"/api/users/{user.Id}/roles/admin", new { });
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Assign same role second time
        var response2 = await _context.Client.PostJsonAsync($"/api/users/{user.Id}/roles/admin", new { });

        // Assert - Second assignment should also succeed
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var userWithRoles = await _context.GetUserWithRolesFromDbAsync(_testUser.Email);
        // User should still have exactly one "admin" role, not duplicated
        userWithRoles.Roles.Count(r => r.Name == "admin").Should().Be(1);
    }

    /// <summary>
    /// Tests the DELETE /api/users/{userId}/roles/{roleName} endpoint removes a role from user.
    /// </summary>
    [Fact]
    public async Task RemoveRoleFromUser_ShouldSucceed()
    {
        // Arrange - Provision user and assign role
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Assign role first
        await _context.Client.PostJsonAsync($"/api/users/{user.Id}/roles/admin", new { });

        // Act - Remove role
        var response = await _context.Client.DeleteAsync($"/api/users/{user.Id}/roles/admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userAfterRemoval = await _context.GetUserWithRolesFromDbAsync(_testUser.Email);
        userAfterRemoval.Roles.Should().NotContain(r => r.Name == "admin");
    }

    /// <summary>
    /// Tests that removing a role that doesn't exist is idempotent.
    /// </summary>
    [Fact]
    public async Task RemoveRoleFromUser_NonExistent_ShouldSucceed()
    {
        // Arrange
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Act - Try to remove role that was never assigned
        var response = await _context.Client.DeleteAsync($"/api/users/{user.Id}/roles/nonexistent");

        // Assert - Should still return success (idempotent)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests the POST /api/users/{userId}/permissions/{permissionName} endpoint grants a permission.
    /// </summary>
    [Fact]
    public async Task GrantPermissionToUser_ShouldSucceed()
    {
        // Arrange - Provision user
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Act - Grant permission
        var response = await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/permissions/users.view",
            new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userWithPerms = await _context.GetUserWithPermissionsFromDbAsync(_testUser.Email);
        userWithPerms.Permissions.Should().Contain(p => p.Name == "users.view");
    }

    /// <summary>
    /// Tests granting permission with a reason query parameter.
    /// </summary>
    [Fact]
    public async Task GrantPermissionToUser_WithReason_ShouldSucceed()
    {
        // Arrange
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        var reason = "Temporary elevated access for project X";

        // Act - Grant permission with reason
        var response = await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/permissions/users.manage_permissions?reason={System.Web.HttpUtility.UrlEncode(reason)}",
            new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests the DELETE /api/users/{userId}/permissions/{permissionName} endpoint revokes a permission.
    /// </summary>
    [Fact]
    public async Task RevokePermissionFromUser_ShouldSucceed()
    {
        // Arrange - Provision user and grant permission
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Grant permission first
        await _context.Client.PostJsonAsync($"/api/users/{user.Id}/permissions/users.delete", new { });

        // Act - Revoke permission
        var response = await _context.Client.DeleteAsync($"/api/users/{user.Id}/permissions/users.delete");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userAfterRevoke = await _context.GetUserWithPermissionsFromDbAsync(_testUser.Email);
        userAfterRevoke.Permissions.Should().NotContain(p => p.Name == "users.delete");
    }

    /// <summary>
    /// Tests that revoking a permission that doesn't exist is idempotent.
    /// </summary>
    [Fact]
    public async Task RevokePermissionFromUser_NonExistent_ShouldSucceed()
    {
        // Arrange
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Act - Try to revoke permission that was never granted
        var response = await _context.Client.DeleteAsync($"/api/users/{user.Id}/permissions/nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests multiple operations on the same user in a single test.
    /// Assigns roles and permissions, then verifies both are present.
    /// </summary>
    [Fact]
    public async Task MultipleOperations_ShouldAllSucceed()
    {
        // Arrange
        var token = await _context.GetTokenAsync(_testUser.Email, _testUser.Password);
        _context.Client.WithBearerToken(token);

        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(_testUser.Email);

        // Act - Multiple operations
        await _context.Client.PostJsonAsync($"/api/users/{user.Id}/roles/admin", new { });
        await _context.Client.PostJsonAsync($"/api/users/{user.Id}/roles/editor", new { });
        await _context.Client.PostJsonAsync($"/api/users/{user.Id}/permissions/users.view", new { });
        await _context.Client.PostJsonAsync($"/api/users/{user.Id}/permissions/users.edit", new { });

        // Assert - All operations should be persisted
        var userFinal = await _context.GetRequiredService<UsersDbContext>()
            .Users
            .Include(u => u.Roles)
            .Include(u => u.Permissions)
            .FirstAsync(u => u.Id == user.Id);

        userFinal.Roles.Should().HaveCount(2);
        userFinal.Roles.Should().Contain(r => r.Name == "admin");
        userFinal.Roles.Should().Contain(r => r.Name == "editor");

        userFinal.Permissions.Should().HaveCount(2);
        userFinal.Permissions.Should().Contain(p => p.Name == "users.view");
        userFinal.Permissions.Should().Contain(p => p.Name == "users.edit");
    }
}
