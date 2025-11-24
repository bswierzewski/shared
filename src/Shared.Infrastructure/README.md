# Shared.Infrastructure

A comprehensive .NET infrastructure library providing a **modular architecture system** with automatic service discovery and registration for building scalable, maintainable applications using Clean Architecture principles.

## 🚀 Key Features

### Automatic Service Discovery & Registration
- **MediatR Handlers**: Automatic discovery and registration from assemblies marked with `IModuleAssembly`
- **FluentValidation Validators**: Automatic validator registration across all module assemblies
- **HTTP Endpoints**: Automatic endpoint mapping via `IModuleEndpoints` interface
- **Assembly Scanning**: Convention-based discovery using marker interfaces

### MediatR Pipeline Behaviors (Automatically Registered)
1. **LoggingBehavior**: Request/response logging with execution timing
2. **UnhandledExceptionBehavior**: Global exception handling and logging
3. **AuthorizationBehavior**: Declarative authorization via `[Authorize]` attributes
4. **ValidationBehavior**: Automatic request validation using FluentValidation
5. **PerformanceBehavior**: Performance monitoring with configurable thresholds

### EF Core Interceptors
- **AuditableEntityInterceptor**: Automatic tracking of `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- **DomainEventDispatcherInterceptor**: Automatic domain event publishing via MediatR

### Module System
- **Dynamic Module Loading**: Automatic module discovery from assemblies
- **Lifecycle Management**: Register → Use → Initialize hooks
- **Configuration-based Enable/Disable**: Control modules via configuration

## 📦 Installation

```bash
dotnet add package Shared.Infrastructure
dotnet add package Shared.Abstractions
```

## 🏗️ Architecture Overview

```
Your Application
├── Program.cs                          # Entry point
├── Modules/
│   ├── Users/
│   │   ├── Users.Domain/              # Domain entities
│   │   ├── Users.Application/         # Commands, Queries, Handlers
│   │   │   └── ApplicationAssembly.cs # Marker for auto-discovery
│   │   └── Users.Infrastructure/      # Module implementation
│   │       ├── InfrastructureAssembly.cs
│   │       ├── UsersModule.cs         # IModule implementation
│   │       ├── UsersDbContext.cs      # EF Core DbContext
│   │       └── Endpoints/
│   │           └── UserEndpoints.cs   # IModuleEndpoints implementation
│   └── Orders/
│       └── ... (same structure)
```

## 🎯 Quick Start

### 1. Configure Your Application

```csharp
// Program.cs
using Shared.Infrastructure.Modules;

var builder = WebApplication.CreateBuilder(args);

// ✨ Register all modules (automatic discovery)
builder.Services.AddModules(builder.Configuration);

// Add your application-specific services
builder.Services.AddControllers();

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();

// ✨ Use all modules (automatic middleware & endpoint configuration)
app.UseModules(builder.Configuration);

app.MapControllers();

// ✨ Initialize all modules (migrations, seeding, etc.)
await app.Services.InitializeApplicationAsync();

await app.RunAsync();
```

That's it! The module system will automatically:
- ✅ Discover all modules in your assemblies
- ✅ Register MediatR handlers from all `IModuleAssembly`-marked assemblies
- ✅ Register FluentValidation validators
- ✅ Register and map HTTP endpoints from `IModuleEndpoints` implementations
- ✅ Configure middleware for each module
- ✅ Run database migrations and initialization logic

## 📘 Creating a New Module

### Step 1: Create the Module Structure

```
Modules/
└── YourModule/
    ├── YourModule.Domain/
    ├── YourModule.Application/
    │   ├── Commands/
    │   ├── Queries/
    │   └── ApplicationAssembly.cs      # ← Marker class
    └── YourModule.Infrastructure/
        ├── InfrastructureAssembly.cs   # ← Marker class
        ├── YourModuleDbContext.cs
        ├── YourModule.cs               # ← IModule implementation
        └── Endpoints/
            └── YourModuleEndpoints.cs  # ← IModuleEndpoints implementation
```

### Step 2: Add Assembly Markers

**Application Layer** (`ApplicationAssembly.cs`):
```csharp
using Shared.Abstractions.Modules;

namespace YourModule.Application;

/// <summary>
/// Marker class for automatic discovery of:
/// - MediatR handlers (commands, queries, notifications)
/// - FluentValidation validators
/// </summary>
public sealed class ApplicationAssembly : IModuleAssembly
{
}
```

**Infrastructure Layer** (`InfrastructureAssembly.cs`):
```csharp
using Shared.Abstractions.Modules;

namespace YourModule.Infrastructure;

/// <summary>
/// Marker class for automatic discovery of:
/// - MediatR handlers
/// - FluentValidation validators
/// - HTTP endpoints (IModuleEndpoints)
/// </summary>
public sealed class InfrastructureAssembly : IModuleAssembly
{
}
```

### Step 3: Implement the Module

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions.Authorization;
using Shared.Abstractions.Modules;

namespace YourModule.Infrastructure;

/// <summary>
/// YourModule - provides [describe your module's purpose]
///
/// Automatic Registration (via IModuleAssembly markers):
/// - MediatR handlers from Application and Infrastructure assemblies
/// - FluentValidation validators
/// - HTTP endpoints via IModuleEndpoints
///
/// This Register() method only handles module-specific services.
/// </summary>
public class YourModule : IModule
{
    public string Name => "yourmodule";

    /// <summary>
    /// Registers module-specific services, DbContext, and configuration.
    /// MediatR handlers, validators, and endpoints are registered automatically.
    /// </summary>
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<YourModuleDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("YourModule")
                ?? throw new InvalidOperationException("Connection string 'YourModule' not found");

            options.UseNpgsql(connectionString)
                   .AddInterceptors(sp.GetServices<Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor>());
        });

        // Register module-specific services
        services.AddScoped<IYourService, YourService>();

        // Configure module-specific options
        services.Configure<YourModuleOptions>(configuration.GetSection("YourModule"));
    }

    /// <summary>
    /// Configures module middleware.
    /// Endpoints are mapped automatically via IModuleEndpoints.
    /// </summary>
    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        // Add module-specific middleware if needed
        // app.UseMiddleware<YourCustomMiddleware>();

        // Endpoints are mapped automatically by ModuleExtensions.UseModules()
    }

    /// <summary>
    /// Initializes the module (runs migrations, seeds data, etc.)
    /// Called automatically during application startup.
    /// </summary>
    public async Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        // Run database migrations
        await new MigrationService<YourModuleDbContext>(serviceProvider).MigrateAsync(cancellationToken);

        // Seed initial data if needed
        // await SeedDataAsync(serviceProvider, cancellationToken);
    }

    /// <summary>
    /// Defines permissions for this module.
    /// These are automatically synchronized to the database during initialization.
    /// </summary>
    public IEnumerable<Permission> GetPermissions()
    {
        return
        [
            new Permission("yourmodule.view", "View items", Name, "View module items"),
            new Permission("yourmodule.create", "Create items", Name, "Create new items"),
            new Permission("yourmodule.edit", "Edit items", Name, "Edit existing items"),
            new Permission("yourmodule.delete", "Delete items", Name, "Delete items"),
        ];
    }

    /// <summary>
    /// Defines roles for this module.
    /// These are automatically synchronized to the database during initialization.
    /// </summary>
    public IEnumerable<Role> GetRoles()
    {
        var permissions = GetPermissions().ToList();

        return
        [
            new Role("admin", "Administrator", Name, permissions.AsReadOnly()),
            new Role("user", "User", Name, permissions.Where(p => p.Name is "yourmodule.view").ToList().AsReadOnly()),
        ];
    }
}
```

### Step 4: Add HTTP Endpoints

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using Shared.Abstractions.Modules;

namespace YourModule.Infrastructure.Endpoints;

/// <summary>
/// HTTP endpoints for YourModule.
/// Automatically discovered and mapped via IModuleEndpoints interface.
/// </summary>
public class YourModuleEndpoints : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/yourmodule")
            .WithTags("YourModule")
            .RequireAuthorization();

        // GET /api/yourmodule/{id}
        group.MapGet("/{id}", GetById)
            .WithName("GetYourModuleItemById")
            .WithOpenApi()
            .Produces<YourItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        // POST /api/yourmodule
        group.MapPost("/", Create)
            .WithName("CreateYourModuleItem")
            .WithOpenApi()
            .Produces<Guid>(StatusCodes.Status201Created);
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator)
    {
        var query = new GetItemByIdQuery(id);
        var result = await mediator.Send(query);

        return result == null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> Create(CreateItemRequest request, IMediator mediator)
    {
        var command = new CreateItemCommand(request.Name, request.Description);
        var id = await mediator.Send(command);

        return Results.Created($"/api/yourmodule/{id}", id);
    }
}
```

### Step 5: Add MediatR Handlers (Auto-Discovered)

**Command Handler** (`Application/Commands/CreateItemCommandHandler.cs`):
```csharp
using MediatR;

namespace YourModule.Application.Commands;

public record CreateItemCommand(string Name, string Description) : IRequest<Guid>;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Guid>
{
    private readonly IYourModuleDbContext _context;

    public CreateItemCommandHandler(IYourModuleDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = new YourItem(request.Name, request.Description);

        _context.Items.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return item.Id;
    }
}
```

✨ **No manual registration needed!** The handler is automatically discovered because the assembly has an `ApplicationAssembly : IModuleAssembly` marker.

### Step 6: Add Validators (Auto-Discovered)

```csharp
using FluentValidation;

namespace YourModule.Application.Commands;

public class CreateItemCommandValidator : AbstractValidator<CreateItemCommand>
{
    public CreateItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
```

✨ **Automatically registered and executed via ValidationBehavior!**

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "YourModule": "Host=localhost;Database=yourmodule_db;Username=user;Password=pass"
  },

  "YourModule": {
    "Module": {
      "Enabled": true  // Set to false to disable the module
    },
    "SomeOption": "value"
  }
}
```

### Disabling a Module

Set `{ModuleName}:Module:Enabled` to `false` in configuration:

```json
{
  "YourModule": {
    "Module": {
      "Enabled": false  // Module will not be loaded
    }
  }
}
```

## 📚 Advanced Usage

### Accessing IUser in Handlers

```csharp
using Shared.Abstractions.Authorization;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Guid>
{
    private readonly IUser _currentUser;
    private readonly IYourModuleDbContext _context;

    public CreateItemCommandHandler(IUser currentUser, IYourModuleDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<Guid> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        // Access current user information
        var userId = _currentUser.Id;
        var userEmail = _currentUser.Email;
        var hasPermission = _currentUser.HasPermission("yourmodule.create");

        // ... create item
    }
}
```

### Authorization with Permissions

```csharp
using Shared.Abstractions.Authorization;

// Require specific permission
[Authorize(Policy = "yourmodule.delete")]
public record DeleteItemCommand(Guid Id) : IRequest;

// Require role
[Authorize(Roles = "admin")]
public record DeleteAllItemsCommand : IRequest;
```

### Custom Middleware in Module

```csharp
public class YourModule : IModule
{
    // ...

    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        // Add custom middleware
        app.Use(async (context, next) =>
        {
            // Custom logic before request
            await next();
            // Custom logic after request
        });

        // Or use a middleware class
        app.UseMiddleware<YourCustomMiddleware>();
    }
}
```

### Domain Events

```csharp
using Shared.Abstractions.Kernel;

public class Item : AggregateRoot
{
    public void Create(string name)
    {
        Name = name;

        // Raise domain event
        AddDomainEvent(new ItemCreatedEvent(Id, name));
    }
}

// Domain event
public record ItemCreatedEvent(Guid ItemId, string Name) : IDomainEvent;

// Domain event handler (auto-discovered)
public class ItemCreatedEventHandler : INotificationHandler<ItemCreatedEvent>
{
    public Task Handle(ItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Handle event (send email, update cache, etc.)
        return Task.CompletedTask;
    }
}
```

Events are automatically dispatched via `DomainEventDispatcherInterceptor` when `SaveChanges()` is called.

## 🎨 Best Practices

### 1. **Module Naming**
- Use lowercase for module names: `"users"`, `"orders"`, `"products"`
- Keep names singular and concise

### 2. **Assembly Organization**
```
✅ GOOD: Place handlers in Application layer
YourModule.Application/
  ├── Commands/
  │   ├── CreateItemCommand.cs
  │   └── CreateItemCommandHandler.cs
  └── Queries/
      ├── GetItemQuery.cs
      └── GetItemQueryHandler.cs

✅ GOOD: Place endpoints in Infrastructure layer
YourModule.Infrastructure/
  └── Endpoints/
      └── YourModuleEndpoints.cs
```

### 3. **Don't Register MediatR Manually**
```csharp
❌ BAD: Manual MediatR registration
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(ApplicationAssembly).Assembly);
});

✅ GOOD: Use IModuleAssembly markers (automatic)
public sealed class ApplicationAssembly : IModuleAssembly { }
```

### 4. **Endpoint Mapping**
```csharp
✅ GOOD: Use IModuleEndpoints
public class YourEndpoints : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { ... }
}

❌ BAD: Manual mapping in Module.Use()
public void Use(IApplicationBuilder app, IConfiguration configuration)
{
    if (app is IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(...); // Don't do this!
    }
}
```

### 5. **Keep Modules Focused**
- Each module should have a single, well-defined responsibility
- Modules should be loosely coupled
- Use domain events for cross-module communication

### 6. **Database Per Module**
- Each module should have its own `DbContext`
- Use separate database schemas or databases for strong isolation
- Share read models if needed, but never write to another module's database

## 🔍 Troubleshooting

### Handlers Not Found

**Problem**: `No service for type 'IRequestHandler<MyCommand, Unit>' has been registered`

**Solution**: Ensure the assembly containing the handler has an `IModuleAssembly` marker class:
```csharp
public sealed class ApplicationAssembly : IModuleAssembly { }
```

### Endpoints Not Mapped

**Problem**: Endpoints return 404 Not Found

**Solution**:
1. Ensure your endpoint class implements `IModuleEndpoints`
2. Ensure the assembly has an `IModuleAssembly` marker
3. Verify `app.UseModules()` is called in Program.cs

### Validators Not Running

**Problem**: Invalid data passes through without validation errors

**Solution**:
1. Ensure validators inherit from `AbstractValidator<TRequest>`
2. Ensure the assembly has an `IModuleAssembly` marker
3. Verify `ValidationBehavior` is registered (automatic with `AddModules()`)

### Module Not Loading

**Problem**: Module is not discovered

**Solution**:
1. Check the module DLL is in the application output directory
2. Ensure the module implements `IModule`
3. Check if module is disabled in configuration (`{ModuleName}:Module:Enabled = false`)
4. Verify the assembly name follows the pattern (contains the module name)

## 📖 API Reference

### IModule Interface

```csharp
public interface IModule
{
    string Name { get; }
    void Register(IServiceCollection services, IConfiguration configuration);
    void Use(IApplicationBuilder app, IConfiguration configuration);
    Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
    IEnumerable<Permission> GetPermissions();
    IEnumerable<Role> GetRoles();
}
```

### IModuleAssembly Interface

```csharp
/// <summary>
/// Marker interface for module assemblies.
/// Assemblies implementing this interface will be automatically scanned for:
/// - MediatR handlers
/// - FluentValidation validators
/// - IModuleEndpoints implementations
/// </summary>
public interface IModuleAssembly { }
```

### IModuleEndpoints Interface

```csharp
/// <summary>
/// Marker interface for automatic endpoint discovery and registration.
/// </summary>
public interface IModuleEndpoints
{
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
```

## 🔗 Dependencies

- **Shared.Abstractions** - Core abstractions (IModule, IModuleAssembly, IModuleEndpoints, etc.)
- **Microsoft.AspNetCore.App** - ASP.NET Core framework
- **Microsoft.EntityFrameworkCore** - EF Core for database access
- **MediatR** - Mediator pattern implementation
- **FluentValidation** - Validation framework

## 📄 License

Apache License 2.0

## 🤝 Contributing

This is a shared infrastructure library. When adding features:
1. Ensure backward compatibility
2. Add comprehensive XML documentation
3. Include usage examples in this README
4. Add unit tests for new functionality
5. Follow existing patterns and conventions

## 📝 Examples

See the `Shared.Users` module in this repository for a complete, production-ready example of:
- ✅ Module structure
- ✅ Assembly markers
- ✅ MediatR handlers (commands, queries)
- ✅ FluentValidation validators
- ✅ HTTP endpoints (IModuleEndpoints)
- ✅ Authentication & Authorization
- ✅ Database migrations
- ✅ Role and permission management
