# Shared.Users

A complete user management module with role-based access control (RBAC), JWT authentication, and just-in-time user provisioning.

## Module Structure

- **Shared.Users.Domain**: Core domain entities and value objects
- **Shared.Users.Application**: Commands, queries, and business logic
- **Shared.Users.Infrastructure**: Database configuration, API endpoints, and JWT services

## Features

- **User Management**: Create, read, update, and delete users
- **Role-Based Access Control**: Assign roles and permissions to users
- **JWT Authentication**: Token generation and validation
- **Just-In-Time Provisioning**: Automatic user creation on first login
- **Permission Checking**: Fine-grained permission management
- **Audit Trail**: Track user creation and modifications

## Installation

Add the NuGet package to your project:

```bash
dotnet add package Shared.Users
```

Or install the three sub-packages separately:
```bash
dotnet add package Shared.Users.Domain
dotnet add package Shared.Users.Application
dotnet add package Shared.Users.Infrastructure
```

## Registration

Register the module in your application:

```csharp
services.AddUserModule(configuration);
```

## Database Setup

The module requires a PostgreSQL database. Update your connection string:

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=shared_users;User Id=postgres;Password=password"
  }
}
```

Run migrations:

```bash
dotnet ef database update --project src/Modules/Shared.Users/Shared.Users.Infrastructure
```

## Configuration

Configure via appsettings.json:

```json
{
  "Authentication": {
    "Jwt": {
      "Secret": "your-secret-key-min-32-chars-long",
      "Issuer": "your-app",
      "Audience": "your-app-users",
      "ExpirationMinutes": 60
    }
  },
  "Users": {
    "DefaultRole": "User",
    "AllowJustInTimeProvisioning": true
  }
}
```

## Usage Examples

### Creating a User

```csharp
public class CreateUserCommand : IRequest<UserDto>
{
    public string Email { get; set; }
    public string FullName { get; set; }
    public List<string> Roles { get; set; }
}

// Use via MediatR
var command = new CreateUserCommand
{
    Email = "user@example.com",
    FullName = "John Doe",
    Roles = new { "User" }
};
var result = await mediator.Send(command);
```

### Authenticating Users

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    var token = await _userService.AuthenticateAsync(request.Email, request.Password);
    return Ok(new { token });
}
```

### Authorization

```csharp
[Authorize]
[HttpGet("profile")]
public async Task<IActionResult> GetProfile()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var user = await _userService.GetUserAsync(int.Parse(userId));
    return Ok(user);
}
```

## Dependencies

- Shared.Abstractions
- Shared.Infrastructure
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.IdentityModel.Tokens

## License

Apache License 2.0
