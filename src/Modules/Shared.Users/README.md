# Shared.Users

A complete, production-ready user management module with role-based access control (RBAC), multi-provider JWT authentication, just-in-time (JIT) user provisioning, and claims enrichment.

## Features

### Core Capabilities
- **Just-In-Time Provisioning**: Automatic user creation on first login from external identity providers
- **Email-Based Provider Linking**: Map multiple external providers to a single internal user account
- **Role-Based Access Control (RBAC)**: Comprehensive role and permission management with support for both direct permissions and role-based permissions
- **Multi-Provider JWT Authentication**: Support for Clerk, Supabase, and custom OAuth providers with OIDC/JWKS discovery
- **Claims Enrichment Pipeline**: Automatic enrichment of authentication claims with user roles and permissions from the database
- **Atomic Caching**: Cache stampede prevention for performance-critical operations

### Module Architecture
The module follows Domain-Driven Design (DDD) and CQRS patterns:
- **Domain Layer**: Core aggregates (User), entities (Role, Permission, ExternalProvider), and domain events
- **Application Layer**: MediatR commands/queries, DTOs, and configuration options
- **Infrastructure Layer**: EF Core DbContext, API endpoints, JWT authentication providers, and JIT provisioning middleware

## Module Structure

```
Shared.Users/
├── Shared.Users.Domain/          # Domain entities and aggregates
├── Shared.Users.Application/      # Commands, queries, and DTOs
└── Shared.Users.Infrastructure/   # Database, endpoints, authentication, and middleware
```

### Key Components

**Infrastructure (UsersModule.cs)**
- Registers DbContext, MediatR handlers, validators, and JWT authentication
- Configures middleware pipeline (JIT provisioning and claims enrichment)
- Initializes database migrations on application startup
- Defines module-level permissions and roles for synchronization

**Database Layer (Persistence/)**
- PostgreSQL database with EF Core 9.0
- Entities: Users, Roles, Permissions, ExternalProviders
- Many-to-many relationships for role and permission assignments
- Unique indices on User.Email and ExternalProvider.(Provider, ExternalUserId)

**Authentication (Extensions/JwtBearers/)**
- Clerk integration with OIDC/JWKS discovery
- Supabase integration with HS256 symmetric key validation
- Custom claim mapping for each provider (email, displayName, pictureUrl)

**Middleware (Middleware/UserProvisioningMiddleware.cs)**
- Validates incoming JWT token from configured authentication provider
- Extracts claims (email, subject, displayName, provider)
- Upserts user in database (JIT provisioning)
- Links external provider to user (email-based matching)
- Loads user roles and permissions from database
- Enriches ClaimsPrincipal with role and permission claims
- Replaces subject (sub) claim with internal user ID for application use

**API Endpoints (Endpoints/UserEndpoints.cs)**
- GET /api/users/{userId} - Retrieve user with roles and permissions
- POST /api/users/{userId}/roles/{roleName} - Assign role to user
- DELETE /api/users/{userId}/roles/{roleName} - Remove role from user
- POST /api/users/{userId}/permissions/{permissionName} - Grant direct permission
- DELETE /api/users/{userId}/permissions/{permissionName} - Revoke direct permission

## Installation

Add the NuGet package to your project:

```bash
dotnet add package Shared.Users
```

Or install sub-packages separately:
```bash
dotnet add package Shared.Users.Domain
dotnet add package Shared.Users.Application
dotnet add package Shared.Users.Infrastructure
```

## Setup & Configuration

### 1. Database Configuration

Configure PostgreSQL connection string in `appsettings.json`:

```json
{
  "Modules": {
    "Users": {
      "ConnectionString": "Server=localhost;Port=5432;Database=shared_users;User Id=postgres;Password=password"
    }
  }
}
```

### 2. Authentication Provider Configuration

Choose your identity provider (Clerk or Supabase):

#### Supabase (HS256)
```json
{
  "Modules": {
    "Users": {
      "Authentication": {
        "Provider": "Supabase"
      },
      "Supabase": {
        "Authority": "https://your-project.supabase.co",
        "JwtSecret": "your-jwt-secret-from-supabase",
        "Audience": "authenticated"
      }
    }
  }
}
```

#### Clerk (OIDC/JWKS)
```json
{
  "Modules": {
    "Users": {
      "Authentication": {
        "Provider": "Clerk"
      },
      "Clerk": {
        "Authority": "https://your-domain.clerk.dev",
        "Audience": "your-audience"
      }
    }
  }
}
```

### 3. Module Registration

Register the module in your application's `Program.cs`:

```csharp
// Load modules from auto-generated registry
var modules = ModuleRegistry.GetModules();

// Register all modules
builder.Services.RegisterModules(modules, builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseModules(modules, builder.Configuration);

// Initialize modules (runs migrations, syncs roles/permissions)
await app.Services.InitializeModules(modules);

app.Run();

// [GenerateModuleRegistry] triggers source generator
[GenerateModuleRegistry]
public partial class Program { }
```

The `Initialize()` method:
- Runs pending database migrations for UsersDbContext
- Synchronizes module-defined roles and permissions to the database
- Is called asynchronously after middleware configuration but before request handling

## Permissions & Roles

### Predefined Permissions

The module automatically defines the following permissions:

- `users.view` - View user information
- `users.create` - Create new users
- `users.edit` - Edit user profiles
- `users.delete` - Delete users
- `users.assign_roles` - Assign roles to users
- `users.manage_permissions` - Grant/revoke permissions to users

### Predefined Roles

- **admin** - Has all permissions
- **editor** - Has users.view, users.edit, users.assign_roles
- **viewer** - Has users.view only

Roles and permissions are automatically synchronized to the database during module initialization.

## Usage Examples

### Accessing Current User

Inject `IUser` to access the current authenticated user and their claims:

```csharp
public class UserController
{
    private readonly IUser _currentUser;

    public UserController(IUser currentUser)
    {
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            UserId = _currentUser.Id,
            Email = _currentUser.Email,
            FullName = _currentUser.FullName,
            PictureUrl = _currentUser.PictureUrl,
            Roles = _currentUser.Roles,
            Permissions = _currentUser.Permissions
        });
    }

    [HttpGet("check-permission")]
    public IActionResult CheckPermission(string permission)
    {
        var hasPermission = _currentUser.HasPermission(permission);
        return Ok(new { hasPermission });
    }
}
```

### Querying Users (via MediatR)

```csharp
// Get user by ID
var query = new GetUserByIdQuery { UserId = userId };
var userDto = await mediator.Send(query);
```

### Managing Roles & Permissions (via MediatR)

```csharp
// Assign role to user
var assignRoleCommand = new AssignRoleToUserCommand
{
    UserId = userId,
    RoleName = "editor"
};
await mediator.Send(assignRoleCommand);

// Grant direct permission to user
var grantPermissionCommand = new GrantPermissionToUserCommand
{
    UserId = userId,
    PermissionName = "users.edit"
};
await mediator.Send(grantPermissionCommand);
```

### Authorization Attributes

Use the `[Authorize]` attribute with policy-based authorization:

```csharp
[Authorize]
[HttpDelete("users/{userId}")]
public async Task<IActionResult> DeleteUser(Guid userId)
{
    // User must have "users.delete" permission (via role or direct grant)
    // Permission check performed by authorization middleware
    var command = new DeleteUserCommand { UserId = userId };
    var result = await mediator.Send(command);
    return Ok(result);
}
```

## How It Works

### JIT Provisioning Flow

1. **Authentication** - Client sends JWT token from Clerk/Supabase/custom provider
2. **JWT Validation** - `ClerkJwtBearerExtensions` or `SupabaseJwtBearerExtensions` validates the token
3. **Claims Extraction** - `UserProvisioningMiddleware` extracts claims: email, subject (sub), displayName, provider
4. **User Upsert** - `UserProvisioningService.UpsertUserAsync()` is called:
   - Search for user by ExternalProvider.ExternalUserId
   - If not found, search by email (to link existing user to new provider)
   - If not found, create new user (JIT provisioning)
   - Update LastLoginAt timestamp
5. **Role & Permission Loading** - Database query loads user's roles and permissions
6. **Claims Enrichment** - `ClaimsPrincipal` is enriched with:
   - `user_id` claim = internal User ID
   - `role` claims for each assigned role
   - `permission` claims for each assigned permission (both role-based and direct)
7. **Request Handling** - Application code accesses current user via `IUser` service, which reads enriched claims

### Role & Permission Synchronization

During module initialization, `RolePermissionSynchronizationService` automatically:
- Collects role and permission definitions from all registered modules via `IModule.GetRoles()` and `IModule.GetPermissions()`
- Syncs definitions to database (adds new, deactivates removed, updates existing)
- Sets `IsModule` and `ModuleName` flags to distinguish module-defined roles/permissions from user-created ones

## Dependencies

### NuGet Packages
- **Microsoft.EntityFrameworkCore** 9.0 - ORM for database access
- **Npgsql.EntityFrameworkCore.PostgreSQL** 9.0 - PostgreSQL provider
- **Microsoft.AspNetCore.Authentication.JwtBearer** 9.0 - JWT authentication
- **Microsoft.IdentityModel.Tokens** - JWT token validation
- **MediatR** - CQRS command/query pattern
- **FluentValidation** - Input validation for commands/queries
- **AutoMapper** - Object mapping for DTOs

### Local Dependencies
- **Shared.Abstractions** - Module interface and authorization abstractions
- **Shared.Infrastructure** - Base migration service and module extension methods

## Database Schema

### Tables
- **Users** - User accounts (unique email index)
- **ExternalProviders** - External identity provider mappings (unique on Provider + ExternalUserId)
- **Roles** - Role definitions (module-managed roles marked with IsModule = true)
- **Permissions** - Permission definitions (module-managed permissions marked with IsModule = true)
- **User_Role** - Many-to-many: Users assigned to Roles
- **User_Permission** - Many-to-many: Users granted direct Permissions
- **Role_Permission** - Many-to-many: Permissions assigned to Roles

### Key Indices
- `Users.Email` (UNIQUE) - Fast lookup by email for provider linking
- `ExternalProviders.Provider + ExternalUserId` (UNIQUE) - Fast lookup by external provider ID
- `Roles.Name`, `Permissions.Name` (UNIQUE) - Fast lookup by name
- `IsActive, IsModule, ModuleName` - Fast filtering for role/permission synchronization

## Testing

The module includes test support via `TestHostProgram`:

```csharp
// Load modules from auto-generated registry
var modules = ModuleRegistry.GetModules();
builder.Services.RegisterModules(modules, builder.Configuration);

// Add test JWT authentication
builder.Services.AddAuthentication()
    .AddTestJwtBearer();

var app = builder.Build();
app.UseModules(modules, builder.Configuration);
await app.Services.InitializeModules(modules);

// [GenerateModuleRegistry] triggers source generator
[GenerateModuleRegistry]
public partial class Program { }
```

Use test JWT bearer tokens to test protected endpoints without external authentication providers.

## License

Apache License 2.0
