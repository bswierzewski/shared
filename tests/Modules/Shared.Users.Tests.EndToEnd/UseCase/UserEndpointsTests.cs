using Shared.Infrastructure.Tests.Core;
using Shared.Infrastructure.Tests.Extensions.Http;
using Shared.Users.Application.DTOs;
using Shared.Users.Infrastructure.Persistence;
using Shared.Users.Tests.EndToEnd.Extensions;
using System.Net;

namespace Shared.Users.Tests.EndToEnd.UseCase;

/// <summary>
/// E2E tests for Users module HTTP endpoints.
/// Tests user retrieval and role management endpoints.
/// </summary>
[Collection("Users")]
public class UserEndpointsTests(UsersSharedFixture fixture) : IAsyncLifetime
{
    private TestContext _context = null!;

    public async Task InitializeAsync()
    {
        _context = await TestContext.CreateBuilder<Program>()
            .WithContainer(fixture.Container)
            .BuildAsync();

        await _context.ResetDatabaseAsync();

        // Get token using fixture's provider (has built-in cache)
        var token = await fixture.TokenProvider.GetTokenAsync(fixture.TestUser.Email, fixture.TestUser.Password);
        _context.Client.WithBearerToken(token);
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

        // Provision user
        await _context.Client.GetAsync("/api/users");

        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);

        // Act - Get user by ID
        var response = await _context.Client.GetAsync($"/api/users/{user.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userDto = await response.ReadAsJsonAsync<UserDto>();
        userDto.Should().NotBeNull();
        userDto!.Id.Should().Be(user.Id);
        userDto.Email.Should().Be(fixture.TestUser.Email);
    }

    /// <summary>
    /// Tests the GET /api/users/{userId} endpoint returns 404 for non-existent user.
    /// </summary>
    [Fact]
    public async Task GetUserById_NonExistent_ShouldReturn404()
    {
        // Act
        var response = await _context.Client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Tests the POST /api/users/{userId}/roles endpoint assigns a role to user.
    /// </summary>
    [Fact]
    public async Task AssignRoleToUser_ShouldSucceed()
    {
        // Arrange - Provision user and get their ID
        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);

        // Get admin role ID
        var rolesResponse = await _context.Client.GetAsync("/api/roles");
        var roles = await rolesResponse.ReadAsJsonAsync<List<RoleDto>>();
        var adminRole = roles!.First(r => r.Name == "admin");

        // Act - Assign role
        var response = await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/roles",
            new { roleIds = new[] { adminRole.Name } });

        // Assert - Endpoint returns success
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify role was assigned in database
        var userWithRoles = await _context.GetUserWithRolesFromDbAsync(fixture.TestUser.Email);
        userWithRoles.Roles.Should().Contain(r => r.Name == "admin");
    }

    /// <summary>
    /// Tests that assigning the same role twice is idempotent (doesn't fail).
    /// </summary>
    [Fact]
    public async Task AssignRoleToUser_Idempotent_SecondAssignmentShouldSucceed()
    {
        // Arrange
        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);

        // Get admin role ID
        var rolesResponse = await _context.Client.GetAsync("/api/roles");
        var roles = await rolesResponse.ReadAsJsonAsync<List<RoleDto>>();
        var adminRole = roles!.First(r => r.Name == "admin");

        // Act - Assign role first time
        var response1 = await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/roles",
            new { roleIds = new[] { adminRole.Name } });
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Assign same role second time
        var response2 = await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/roles",
            new { roleIds = new[] { adminRole.Name } });

        // Assert - Second assignment should also succeed
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var userWithRoles = await _context.GetUserWithRolesFromDbAsync(fixture.TestUser.Email);
        // User should still have exactly one "admin" role, not duplicated
        userWithRoles.Roles.Count(r => r.Name == "admin").Should().Be(1);
    }

    /// <summary>
    /// Tests the DELETE /api/users/{userId}/roles/{roleId} endpoint removes a role from user.
    /// </summary>
    [Fact]
    public async Task RemoveRoleFromUser_ShouldSucceed()
    {
        // Arrange - Provision user and assign role
        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);

        // Get admin role ID
        var rolesResponse = await _context.Client.GetAsync("/api/roles");
        var roles = await rolesResponse.ReadAsJsonAsync<List<RoleDto>>();
        var adminRole = roles!.First(r => r.Name == "admin");

        // Assign role first
        await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/roles",
            new { roleIds = new[] { adminRole.Name } });

        // Act - Remove role
        var response = await _context.Client.DeleteAsync($"/api/users/{user.Id}/roles/{adminRole.Name}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userAfterRemoval = await _context.GetUserWithRolesFromDbAsync(fixture.TestUser.Email);
        userAfterRemoval.Roles.Should().NotContain(r => r.Name == "admin");
    }

    /// <summary>
    /// Tests that removing a role that doesn't exist is idempotent.
    /// </summary>
    [Fact]
    public async Task RemoveRoleFromUser_NonExistent_ShouldSucceed()
    {
        // Arrange
        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);

        // Act - Try to remove role with random GUID that doesn't exist
        var nonExistentRoleId = Guid.NewGuid();
        var response = await _context.Client.DeleteAsync($"/api/users/{user.Id}/roles/{nonExistentRoleId}");

        // Assert - Should still return success (idempotent)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Tests multiple role assignments on the same user in a single test.
    /// Assigns multiple roles, then verifies they are present.
    /// </summary>
    [Fact]
    public async Task MultipleOperations_ShouldAllSucceed()
    {
        // Arrange
        await _context.Client.GetAsync("/api/users");
        var user = await _context.GetUserFromDbAsync(fixture.TestUser.Email);

        // Get all roles to find admin and editor role IDs
        var rolesResponse = await _context.Client.GetAsync("/api/roles");
        rolesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await rolesResponse.ReadAsJsonAsync<List<RoleDto>>();
        var adminRole = roles!.First(r => r.Name == "admin");
        var editorRole = roles!.First(r => r.Name == "editor");

        // Act - Assign multiple roles at once
        var response = await _context.Client.PostJsonAsync(
            $"/api/users/{user.Id}/roles",
            new { roleIds = new[] { adminRole.Name, editorRole.Name } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - All roles should be persisted
        var userFinal = await _context.GetRequiredService<UsersDbContext>()
            .Users
            .Include(u => u.Roles)
            .FirstAsync(u => u.Id == user.Id);

        userFinal.Roles.Should().HaveCount(2);
        userFinal.Roles.Should().Contain(r => r.Name == "admin");
        userFinal.Roles.Should().Contain(r => r.Name == "editor");
    }
}
