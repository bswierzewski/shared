# Shared Infrastructure - Foundation for Clean Architecture Applications

A comprehensive collection of foundational NuGet packages for building modular, scalable .NET 9.0 applications using Clean Architecture patterns, Domain-Driven Design (DDD), and CQRS.

## Overview

These packages provide the essential building blocks and infrastructure for enterprise .NET applications:

- **Shared.Abstractions** - Domain layer contracts and DDD building blocks
- **Shared.Infrastructure** - Cross-cutting concerns and infrastructure implementations
- **Shared.Infrastructure.Tests** - Integration testing foundation and utilities
- **Shared.EnvFileGenerator** - CLI tool for .env file generation from configuration

## Quick Start

### Installation

Add these NuGet packages to your project:

```bash
dotnet add package Shared.Abstractions
dotnet add package Shared.Infrastructure
dotnet add package Shared.Infrastructure.Tests      # For test projects only
dotnet add package Shared.EnvFileGenerator          # As a global tool
```

Or install Shared.EnvFileGenerator as a global tool:

```bash
dotnet tool install -g Shared.EnvFileGenerator
```

### Basic Setup in Program.cs

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Modules;

var builder = WebApplication.CreateBuilder(args);

// Load modular architecture modules
var modules = ModuleLoader.LoadModules();

// Register all modules
builder.Services.RegisterModules(modules, builder.Configuration);

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();

// Use modular architecture
app.UseModules(modules, builder.Configuration);

// Initialize modules (runs migrations, syncs configuration)
await app.Services.InitializeModules(modules);

app.Run();
```

---

## 1. Shared.Abstractions

**NuGet Package:** `Shared.Abstractions` v1.0.0

### Purpose

Provides the foundational domain layer library for Domain-Driven Design (DDD) and Clean Architecture applications. Contains abstract interfaces, base classes, and contracts that define core domain patterns and system-wide contracts.

### Key Features

#### Domain-Driven Design (DDD) Building Blocks

**Base Classes:**

- `AggregateRoot<TId>` - Base class for aggregate roots managing domain events
- `Entity<TId>` - Base entity class with unique identifier
- `AuditableEntity<TId>` - Entity with CreatedAt, CreatedBy, ModifiedAt, ModifiedBy tracking
- `ValueObject` - Abstract base class for value objects with equality based on properties
- `Enumeration<TEnum>` - Type-safe enumeration pattern with GetAll(), FromValue(), FromName()
- `DomainEvent` - Base class for domain events with Id and OccurredOn properties

**Interfaces:**

- `IAggregateRoot` - Marker interface for aggregate roots
- `IDomainEvent` - Contract for domain events (extends MediatR.INotification)
- `IHasDomainEvent` - Collection interface for managing domain events
- `IAuditable` - Contract for entities requiring audit tracking
- `IBusinessRule` - Contract for domain business rule validation
- `IModuleAssembly` - Marker interface for module assemblies

#### Authorization & Security

- `IUser` - Current authenticated user context with:
  - Properties: Id, Email, FullName, PictureUrl, IsAuthenticated, Claims, Roles, Permissions
  - Methods: IsInRole(), HasClaim(), HasPermission()
- `[Authorize]` - Attribute for MediatR requests specifying required Roles (OR logic), Permissions (AND logic), Claims (AND logic)
- `Permission` - Record type for permission definitions
- `Role` - Record type for role definitions with permissions

#### Modular Architecture

- `IModule` - Interface for modular architecture with:
  - `Name` property - Unique module identifier
  - `Register()` - Service registration (called during startup)
  - `Use()` - Middleware configuration (called during app building)
  - `Initialize()` - Async initialization (runs migrations, syncs config, etc.)
  - `GetPermissions()` - Returns module-defined permissions
  - `GetRoles()` - Returns module-defined roles

#### Configuration Pattern

- `IOptions<T>` - Marker interface with static abstract `SectionName` property for configuration binding

#### Exceptions

- `DomainException` - Base exception for domain-specific errors
- `BusinessRuleValidationException` - Thrown when business rules are violated

### Usage Examples

#### Creating a Domain Entity with Events

```csharp
using Shared.Abstractions.Abstractions;
using Shared.Abstractions.Primitives;

public class Order : AggregateRoot<Guid>
{
    public string OrderNumber { get; set; }
    public List<OrderItem> Items { get; set; }

    public static Order Create(string orderNumber, List<OrderItem> items)
    {
        var order = new Order { Id = Guid.NewGuid(), OrderNumber = orderNumber, Items = items };
        order.AddDomainEvent(new OrderCreatedEvent { Order = order });
        return order;
    }
}

public class OrderCreatedEvent : DomainEvent
{
    public Order Order { get; set; }
}
```

#### Implementing Business Rules

```csharp
public class OrderMustHaveItemsRule : IBusinessRule
{
    private readonly List<OrderItem> _items;

    public OrderMustHaveItemsRule(List<OrderItem> items)
    {
        _items = items;
    }

    public string Message => "Order must have at least one item";
    public bool IsBroken() => !_items.Any();
}

// Usage in aggregate root
order.CheckRule(new OrderMustHaveItemsRule(order.Items));
```

#### Creating a Modular Architecture Module

```csharp
using Shared.Abstractions.Modules;
using Shared.Abstractions.Authorization;

public class OrdersModule : IModule
{
    public string Name => "orders";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(OrdersModule).Assembly));
    }

    public void Use(IApplicationBuilder app, IConfiguration configuration)
    {
        if (app is IEndpointRouteBuilder endpoints)
        {
            endpoints.MapOrderEndpoints();
        }
    }

    public async Task Initialize(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        // Run module initialization (migrations, seeding, etc.)
    }

    public IEnumerable<Permission> GetPermissions()
    {
        return new[]
        {
            new Permission("orders.view", "View Orders", Name, "View order information"),
            new Permission("orders.create", "Create Orders", Name, "Create new orders"),
        };
    }

    public IEnumerable<Role> GetRoles()
    {
        return new[]
        {
            new Role("admin", "Administrator", Name, GetPermissions().ToList().AsReadOnly()),
        };
    }
}
```

#### Authorizing MediatR Commands

```csharp
using Shared.Abstractions.Authorization;

[Authorize(Roles = "admin", Permissions = "orders.create")]
public class CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public string OrderNumber { get; set; }
    public List<OrderItemDto> Items { get; set; }
}
```

### Dependencies

- MediatR
- Microsoft.EntityFrameworkCore
- Microsoft.AspNetCore.App (framework reference)

---

## 2. Shared.Infrastructure

**NuGet Package:** `Shared.Infrastructure` v1.0.0

### Purpose

Provides infrastructure layer implementations for cross-cutting concerns including authentication, authorization, validation, logging, performance monitoring, domain event dispatching, and modular architecture support.

**Depends on:** Shared.Abstractions

### Key Features

#### MediatR Pipeline Behaviors

**AuthorizationBehavior**

- Validates authorization attributes on requests
- Checks roles (OR logic), permissions (AND logic), claims (AND logic)
- Throws `UnauthorizedAccessException` if not authenticated
- Throws `ForbiddenAccessException` if lacks permission

**ValidationBehavior**

- Runs FluentValidation validators on all requests
- Throws `ValidationException` with grouped errors if validation fails

**PerformanceBehavior**

- Monitors request execution time
- Logs warnings for slow requests (default > 3 seconds)

**LoggingBehavior**

- Logs request start and completion with duration
- Includes request details in debug mode

**UnhandledExceptionBehavior**

- Catches and logs unhandled exceptions
- Re-throws exception for standard error handling

#### Result Pattern Implementation

**Error**

```csharp
// Create error
var error = Error.Create("INVALID_EMAIL", "Email format is invalid");

// Or use helper
var error = Error.FromMessage("Something went wrong");
```

**Result<T>** - Generic result type for functional programming

```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Return success
        return Result.Success(new OrderDto { /* ... */ });

        // Or return failure
        return Result.Failure<OrderDto>(Error.Create("ORDER_FAILED", "Failed to create order"));
    }
}
```

**PaginatedList<T>**

```csharp
var query = orders.AsQueryable();
var paginated = await PaginatedList<Order>.CreateAsync(query, pageNumber: 1, pageSize: 10);

return Ok(new
{
    Items = paginated.Items,
    PageNumber = paginated.PageNumber,
    TotalPages = paginated.TotalPages,
    TotalCount = paginated.TotalCount,
    HasPreviousPage = paginated.HasPreviousPage,
    HasNextPage = paginated.HasNextPage,
});
```

#### Functional Programming Extensions

```csharp
var result = Result.Success(5)
    .Bind(x => x > 0 ? Result.Success(x * 2) : Result.Failure<int>(Error.FromMessage("Must be positive")))
    .Map(x => x.ToString())
    .OnSuccess(value => Console.WriteLine($"Success: {value}"))
    .OnFailure(errors => Console.WriteLine($"Failed: {errors[0].Message}"));
```

#### Entity Framework Core Interceptors

**AuditableEntityInterceptor**

- Automatically sets CreatedAt, CreatedBy, ModifiedAt, ModifiedBy on entities
- Reads current user from IUser service via HttpContext
- Handles JIT scenarios where system creates users (CreatedBy = null)

**DispatchDomainEventsInterceptor**

- Automatically publishes domain events via MediatR after SaveChanges
- Prevents duplicate event handling with internal tracking
- Publishes events before SaveChanges completes (transactional consistency)

#### Modular Architecture Support

**ModuleLoader**

- Discovers all IModule implementations via reflection
- Supports assembly exclusion by prefix (System._, Microsoft._, etc.)
- Example:

```csharp
var modules = ModuleLoader.LoadModules();
// Or with custom exclusions
var modules = ModuleLoader.LoadModules(exclusionPrefixes: new[] { "Legacy." });
```

**ModuleExtensions**

```csharp
// Register all modules
builder.Services.RegisterModules(modules, builder.Configuration);

// Configure middleware
app.UseModules(modules, builder.Configuration);

// Initialize modules (async)
await app.Services.InitializeModules(modules);
```

#### Configuration Extensions

```csharp
// Bind configuration section to IOptions
var smtpOptions = configuration.LoadOptions<SmtpOptions>();

// Bind with specific section
services.Configure<SmtpOptions>(
    configuration.GetSection(SmtpOptions.SectionName));
```

#### Exception Handling

**ForbiddenAccessException**

```csharp
if (!user.HasPermission("users.write"))
{
    throw new ForbiddenAccessException("You do not have permission to write users");
}
```

**ValidationException**

```csharp
throw new ValidationException(new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required" } }
});
```

### Setup in Program.cs

```csharp
// Add MediatR with behaviors
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);

    // Add pipeline behaviors in order
    config.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(PerformanceBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
    config.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
});

// Add validators
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add DbContext with interceptors
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString)
           .AddInterceptors(
               sp.GetService<AuditableEntityInterceptor>(),
               sp.GetService<DispatchDomainEventsInterceptor>());
});

// Register interceptors
builder.Services.AddScoped<AuditableEntityInterceptor>();
builder.Services.AddScoped<DispatchDomainEventsInterceptor>();
```

### Dependencies

- Shared.Abstractions
- MediatR
- FluentValidation
- Microsoft.EntityFrameworkCore
- Microsoft.AspNetCore.App (framework reference)

---

## 3. Shared.Infrastructure.Tests

**NuGet Package:** `Shared.Infrastructure.Tests` v1.0.0

### Purpose

Provides base classes and utilities for integration testing infrastructure components. Includes WebApplicationFactory with PostgreSQL test container, database reset utilities, and HTTP client extensions for E2E testing.

**Depends on:** Shared.Infrastructure, Shared.Abstractions

### Key Features

#### TestBase Class

Abstract base class for all integration tests with automatic lifecycle management:

```csharp
public abstract class UserServiceTests(ITestWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Properties: Client (HttpClient), Services (IServiceProvider)
        var response = await Client.GetAsync($"/api/users/{userId}");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleTests_UseFreshDatabase()
    {
        // Each test automatically resets database via IAsyncLifetime
        // Tables preserved: Roles, Permissions (configured in TablesToIgnoreOnReset)
    }
}
```

**Features:**

- Automatic database reset between tests via ResetDatabasesAsync()
- HttpClient for making requests: `Client`
- Service provider for dependency resolution: `Services`
- Virtual hooks: OnConfigureServices(), OnInitializeAsync(), RequiresServiceCustomization()
- Helper methods: CreateScope(), Resolve<T>(), RegisterMock<T>()
- IAsyncLifetime implementation for async setup/teardown

#### TestWebApplicationFactory

Generic factory managing PostgreSQL test container:

```csharp
public class ApplicationFactory : TestWebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Override specific configuration
        builder.ConfigureServices(services =>
        {
            // Register test doubles, mocks, etc.
        });
    }

    protected override string[] TablesToIgnoreOnReset => new[]
    {
        "Roles",
        "Permissions",
        "Users" // Keep test users
    };
}
```

**Features:**

- Starts PostgreSQL 16 test container automatically
- Creates ephemeral test databases
- Overrides all connection strings to test container
- Configures ephemeral Data Protection
- Automatic database cleanup on disposal
- Respawn for efficient table truncation between tests

#### HTTP Client Extensions

```csharp
// Send JSON request and deserialize response
var response = await Client.PostJsonAsync<UserDto>(
    "/api/users",
    new { Email = "test@example.com" });

var user = await response.ReadAsJsonAsync<UserDto>();

// Work with authorization
var authenticatedClient = Client.WithBearerToken("jwt-token");
var response = await authenticatedClient.GetAsync("/api/me");

// Remove authorization
var publicClient = Client.WithoutAuthorization();
```

**Extension Methods:**

- `PostJsonAsync<T>(url, data)` - POST with JSON serialization
- `PutJsonAsync<T>(url, data)` - PUT with JSON serialization
- `ReadAsJsonAsync<T>()` - Deserialize response as JSON (case-insensitive)
- `WithBearerToken(token)` - Add JWT Bearer token
- `WithoutAuthorization()` - Remove Authorization header

#### Service Collection Extensions

```csharp
public class OrderServiceTests(ITestWebApplicationFactory factory) : TestBase(factory)
{
    protected override void OnConfigureServices(IServiceCollection services)
    {
        // Register mock for external service
        RegisterMock<IEmailService>();

        // Or register custom test implementation
        RegisterService<IOrderRepository, InMemoryOrderRepository>();
    }
}
```

#### Module Initialization

Automatically loads `.env` files from test output directory:

```csharp
[ModuleInitializer]
public static void Init()
{
    DotNetEnv.Env.Load(Path.Combine(AppContext.BaseDirectory, ".env.test"));
}
```

### Usage Example

```csharp
using Shared.Infrastructure.Tests;
using Xunit;
using FluentAssertions;

public class CreateUserTests(ApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreatedUser()
    {
        // Arrange
        var command = new { Email = "test@example.com", FullName = "Test User" };

        // Act
        var response = await Client.PostJsonAsync<UserDto>("/api/users", command);

        // Assert
        response.Should().NotBeNull();
        response.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var command = new { Email = "test@example.com", FullName = "Test User" };
        await Client.PostJsonAsync<UserDto>("/api/users", command); // First user

        // Act
        var response = await Client.PostJsonAsync("/api/users", command); // Duplicate

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUser_WithoutAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        var client = Client.WithoutAuthorization();

        // Act
        var response = await client.GetAsync($"/api/users/some-id");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
```

### Setup for Test Projects

**In test project .csproj:**

```xml
<ItemGroup>
    <PackageReference Include="Shared.Infrastructure.Tests" Version="1.0.0" />
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.2" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
</ItemGroup>
```

**Create fixture for your application:**

```csharp
public class ApplicationFactory : TestWebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
    }
}
```

### Dependencies

- Shared.Infrastructure
- Shared.Abstractions
- xunit, FluentAssertions, Moq (exported)
- Testcontainers, Testcontainers.PostgreSql (internal)
- Respawn (internal)
- Microsoft.AspNetCore.Mvc.Testing

---

## 4. Shared.EnvFileGenerator

**NuGet Package:** `Shared.EnvFileGenerator` v1.0.0 (Global Tool)

### Purpose

CLI utility tool for generating, listing, and updating `.env` files from `IOptions` implementations. Scans compiled assemblies to automatically discover configuration requirements.

**Depends on:** Shared.Abstractions

### Installation

```bash
dotnet tool install -g Shared.EnvFileGenerator
```

Then use anywhere: `shared env [command]`

### Commands

#### 1. Generate Command

Generates a `.env.example` file from all IOptions implementations:

```bash
shared env generate [options]

Options:
  -p, --path              Project path to scan (default: current directory)
  -o, --output            Output .env file path (default: .env.example)
  -c, --config            Build configuration (Debug|Release, default: Debug)
  -r, --recursive         Scan referenced projects (default: false)
  -d, --descriptions      Include type descriptions in comments (default: false)
  -f, --force             Overwrite existing file (default: false)
```

**Example:**

```bash
# Generate from current project
shared env generate

# Generate with descriptions
shared env generate -d true -o .env.local

# Scan referenced projects
shared env generate -r true

# Release build
shared env generate -c Release
```

**Output Format:**

```env
# ==========================================
# Database
# ==========================================
# Type: string
DATABASE__CONNECTIONSTRING=

# Type: string
DATABASE__HOST=localhost

# Type: int
DATABASE__PORT=5432

# ==========================================
# Smtp
# ==========================================
# Type: string
SMTP__HOST=

# Type: int
SMTP__PORT=587

# Type: string
SMTP__USERNAME=

# Type: string
SMTP__PASSWORD=
```

#### 2. List Command

Lists all available configuration sections and properties:

```bash
shared env list [options]

Options:
  -p, --path              Project path to scan (default: current directory)
  -c, --config            Build configuration (Debug|Release, default: Debug)
  -r, --recursive         Scan referenced projects (default: false)
  -v, --verbose           Show detailed property information (default: false)
```

**Example:**

```bash
# List all sections
shared env list

# List with property details
shared env list -v

# List from release build
shared env list -c Release -v
```

**Output:**

```
Found 3 configuration sections:

1. Database
   - ConnectionString (string)
   - Host (string)
   - Port (int)

2. Smtp
   - Host (string)
   - Port (int)
   - Username (string)
   - Password (string)

3. Authentication
   - Issuer (string)
   - Audience (string)
   - ExpirationMinutes (int)
```

#### 3. Update Command

Merges new configuration sections with existing `.env` file, preserving current values:

```bash
shared env update [options]

Options:
  -p, --path              Project path to scan (default: current directory)
  -e, --env-file          Path to .env file to update (default: .env)
  -c, --config            Build configuration (Debug|Release, default: Debug)
  -r, --recursive         Scan referenced projects (default: false)
  -b, --backup            Create backup before updating (default: false)
  -d, --descriptions      Include type descriptions (default: false)
```

**Example:**

```bash
# Update local .env file
shared env update -e .env.local

# Create backup before updating
shared env update -b true

# Update with descriptions
shared env update -d true
```

### How It Works

1. **Assembly Scanning**: Scans `bin/Debug` or `bin/Release` folder for compiled DLLs
2. **IOptions Discovery**: Finds all classes implementing `IOptions` interface
3. **Configuration Extraction**: Reads static `SectionName` property and public properties
4. **Environment Variable Generation**: Creates `SECTIONNAME__PROPERTYNAME=value` format
5. **Merging**: For update, preserves existing values while adding new sections

### Creating IOptions Classes

```csharp
using Shared.Abstractions.Options;
using Microsoft.Extensions.Options;

public class SmtpOptions : IOptions
{
    public static string SectionName => "Smtp";

    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class DatabaseOptions : IOptions
{
    public static string SectionName => "Database";

    public string ConnectionString { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
}
```

### Configuration Binding in appsettings.json

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=myapp;User Id=postgres;Password=password",
    "Host": "localhost",
    "Port": 5432
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "noreply@example.com",
    "Password": "app-password"
  },
  "Authentication": {
    "Issuer": "https://example.com",
    "Audience": "myapp",
    "ExpirationMinutes": 60
  }
}
```

### Environment Variable Binding

When using `.env` files with environment variables:

```bash
DATABASE__CONNECTIONSTRING=Server=prod-db.example.com;...
DATABASE__HOST=prod-db.example.com
DATABASE__PORT=5432
SMTP__HOST=smtp.gmail.com
SMTP__PORT=587
SMTP__USERNAME=noreply@example.com
SMTP__PASSWORD=xxxxx
```

These are automatically bound to IOptions via .NET configuration binder.

### Workflow Example

```bash
# 1. Create your project with IOptions classes
# 2. Build it
dotnet build

# 3. Generate initial .env.example
shared env generate -o .env.example -d true

# 4. Copy template to .env and update values
cp .env.example .env
# Edit .env with actual values

# 5. Later, when adding new options, update .env
shared env update -e .env -b true
```

### Dependencies

- Shared.Abstractions
- System.CommandLine (for CLI)

---

## Dependency Graph

```
┌─────────────────────────────────────────────────────────────┐
│ Shared.Abstractions                                         │
│ (Domain contracts, DDD building blocks, IModule interface)  │
└─────────┬───────────────────────────────────────────────────┘
          │
          │ depends on
          │
┌─────────▼───────────────────────────────────────────────────┐
│ Shared.Infrastructure                                       │
│ (MediatR behaviors, interceptors, module system)            │
└─────────┬───────────────────────────────────────────────────┘
          │
          │ depends on
          │
┌─────────▼───────────────────────────────────────────────────┐
│ Shared.Infrastructure.Tests                                 │
│ (TestBase, WebApplicationFactory, test utilities)           │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ Shared.EnvFileGenerator (CLI Tool)                          │
│ (only depends on Shared.Abstractions)                       │
└─────────────────────────────────────────────────────────────┘
```

## Integration with Modular Modules

These packages provide the foundation for building modular applications. Modules (like `Shared.Users`) implement `IModule` interface and:

1. Are discovered automatically via reflection
2. Register their own services in `Register()`
3. Configure their own middleware in `Use()`
4. Run initialization tasks in `Initialize()`
5. Define their own permissions and roles via `GetPermissions()` and `GetRoles()`

**Example module structure:**

```
MyModule/
├── MyModule.Domain/
│   └── Aggregates, Entities, Domain Events
├── MyModule.Application/
│   └── Commands, Queries, DTOs
└── MyModule.Infrastructure/
    ├── MyModule.cs (implements IModule)
    ├── Persistence/ (DbContext, Migrations)
    ├── Endpoints/
    └── Services/
```

---

## Common Setup Patterns

### Pattern 1: Command-Query Architecture with Validation

```csharp
// Define command
[Authorize(Permissions = "orders.create")]
public class CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public string OrderNumber { get; set; }
}

// Add validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderNumber).NotEmpty().MaximumLength(50);
    }
}

// Handle command
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _repository;

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(request.OrderNumber);
        await _repository.AddAsync(order, cancellationToken);
        return Result.Success(new OrderDto { OrderNumber = order.OrderNumber });
    }
}

// Send from controller/endpoint
var result = await mediator.Send(new CreateOrderCommand { OrderNumber = "ORD-001" });
```

### Pattern 2: Paginated Query Results

```csharp
public class GetOrdersQuery : IRequest<Result<PaginatedList<OrderDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<PaginatedList<OrderDto>>>
{
    private readonly IOrderRepository _repository;

    public async Task<Result<PaginatedList<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetAll();
        var paginated = await PaginatedList<Order>.CreateAsync(query, request.PageNumber, request.PageSize);

        return Result.Success(new PaginatedList<OrderDto>(
            paginated.Items.Select(o => new OrderDto { /* ... */ }).ToList(),
            paginated.PageNumber,
            paginated.PageSize,
            paginated.TotalCount));
    }
}
```

### Pattern 3: Database Entities with Audit Tracking

```csharp
public class Order : AuditableEntity<Guid>
{
    public string OrderNumber { get; set; }
    // Inherited: CreatedAt, CreatedBy, ModifiedAt, ModifiedBy
}

// Configure in DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>().Property(o => o.CreatedAt).IsRequired();
    modelBuilder.Entity<Order>().Property(o => o.CreatedBy).IsRequired(false);
}
```

### Pattern 4: Integration Testing with Test Factory

```csharp
public class ApplicationFactory : TestWebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Mock external services for tests
            services.RemoveAll(typeof(IEmailService));
            services.AddScoped(_ => new Mock<IEmailService>().Object);
        });
    }
}

public class OrderServiceTests(ApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsSuccess()
    {
        var response = await Client.PostJsonAsync<OrderDto>("/api/orders", new { OrderNumber = "ORD-001" });
        response.OrderNumber.Should().Be("ORD-001");
    }
}
```

---

## Configuration Best Practices

### 1. Use IOptions for Strong-Typed Configuration

```csharp
public class EmailOptions : IOptions
{
    public static string SectionName => "Email";
    public string ApiKey { get; set; }
    public string FromAddress { get; set; }
    public bool Enabled { get; set; }
}

// In service
public class EmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }
}
```

### 2. Generate .env Files Automatically

```bash
# Build project
dotnet build

# Generate .env.example
shared env generate -d true -o .env.example

# Create .env and populate values
cp .env.example .env
# Edit .env with environment-specific values

# Load in Program.cs
DotNetEnv.Env.Load(".env");
var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();
```

### 3. Separate Environments

```
.env.local          # Local development (ignored in git)
.env.test           # Test environment values
.env.production      # Production values (separate secure storage)
.env.example         # Template (committed to git)
```

---

## Troubleshooting

### "No modules found" during application startup

- Ensure module class implements `IModule` interface
- Verify module assembly is loaded (check ModuleLoader exclusions)
- Confirm module class is public and not abstract

### Domain events not being published

- Ensure `DispatchDomainEventsInterceptor` is registered
- Verify EF Core SaveChanges/SaveChangesAsync is called
- Check that domain events are added via `AddDomainEvent()`

### Authorization always fails

- Verify `IUser` implementation is registered
- Check that JWT token contains required claims
- Ensure `AuthorizationBehavior` is registered before request handler

### Tests fail with database connection errors

- PostgreSQL test container must be available (Docker)
- Check `TablesToIgnoreOnReset` doesn't include required tables
- Verify database is properly reset between tests

### EnvFileGenerator doesn't find configurations

- Build project first: `dotnet build`
- Verify IOptions classes implement `Shared.Abstractions.Options.IOptions`
- Check static `SectionName` property exists
- Run from project directory or specify with `-p` flag

---

## License

Apache License 2.0

All packages are part of the Shared Infrastructure ecosystem and designed to work together to build scalable, maintainable .NET applications following Clean Architecture and DDD principles.

## Support & Contributing

For issues, questions, or contributions, please refer to the project repository.
