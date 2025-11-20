# Shared.Infrastructure.Tests

Integration and unit tests for the Shared Infrastructure module. Provides test base classes, factories, and utilities for testing infrastructure components.

## Structure

```
Shared.Infrastructure.Tests/
├── Extensions/
│   ├── ServiceCollectionExtensions.cs    # Service registration helpers for tests
│   └── HttpClientExtensions.cs           # HTTP client utilities for API testing
├── Factories/
│   ├── ITestWebApplicationFactory.cs     # Factory interface for test applications
│   └── TestWebApplicationFactory.cs      # Generic factory with PostgreSQL container management
├── ModuleInitializers/
│   └── EnvironmentModuleInitializer.cs   # Loads .env files before tests run
├── Unit/                                  # Unit tests directory
└── TestBase.cs                           # Base class for integration tests
```

## Key Components

### TestBase

Abstract base class for integration tests. Provides:

- **Automatic database reset** between tests
- **HTTP client** for API testing
- **Service resolution** utilities
- **Mock registration** helpers
- **Test lifecycle hooks**

#### Usage

```csharp
public class MyIntegrationTests(ITestWebApplicationFactory factory) : TestBase(factory)
{
    [Fact]
    public async Task MyTest()
    {
        // Use Client to make HTTP requests
        var response = await Client.GetAsync("/api/endpoint");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Use Resolve<T> to get services
        var service = Resolve<IMyService>();

        // Use CreateScope for scoped services
        using var scope = CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
    }
}
```

### TestWebApplicationFactory<TProgram>

Generic factory managing:

- **PostgreSQL test container** with automatic startup and cleanup
- **Database migrations** for registered DbContext types
- **Respawn** for database cleanup between tests
- **Service customization** hooks
- **Data Protection** configuration for tests

#### Usage

```csharp
public class MyWebApplicationFactory : TestWebApplicationFactory<Program>
{
    protected override Type[] DbContextTypes =>
    [
        typeof(MyDbContext),
        typeof(AnotherDbContext)
    ];

    protected override void OnConfigureDbContexts(IServiceCollection services, string connectionString)
    {
        services.ReplaceDbContext<MyDbContext>(connectionString);
        services.ReplaceDbContext<AnotherDbContext>(connectionString);
    }

    protected override void OnConfigureServices(IServiceCollection services)
    {
        // Add test-specific service configurations
        var mockAuthService = services.RegisterMock<IAuthService>();
        mockAuthService.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(new User { Id = "test-user" });
    }
}
```

### Service Collection Extensions

#### ReplaceDbContext<TContext>

Replaces DbContext registration with test connection string:

```csharp
services.ReplaceDbContext<MyDbContext>(connectionString);
```

#### RegisterMock<TService>

Creates and registers a mock service:

```csharp
var mock = services.RegisterMock<IMyService>();
mock.Setup(x => x.DoSomething()).Returns("test value");
```

#### RegisterService<TService, TImplementation>

Registers a service with singleton lifetime:

```csharp
services.RegisterService<IMyService, TestImplementation>();
```

### HTTP Client Extensions

#### PostJsonAsync<T>

Posts JSON data:

```csharp
var request = new CreateUserRequest { Name = "John" };
var response = await Client.PostJsonAsync("/api/users", request);
```

#### PutJsonAsync<T>

Puts JSON data:

```csharp
var request = new UpdateUserRequest { Name = "Jane" };
var response = await Client.PutJsonAsync("/api/users/123", request);
```

#### ReadAsJsonAsync<T>

Reads JSON response:

```csharp
var response = await Client.GetAsync("/api/users/123");
var user = await response.ReadAsJsonAsync<UserDto>();
```

#### WithBearerToken

Adds authorization header:

```csharp
var token = "jwt-token-here";
Client.WithBearerToken(token);
var response = await Client.GetAsync("/api/protected");
```

#### WithoutAuthorization

Removes authorization header:

```csharp
Client.WithoutAuthorization();
var response = await Client.GetAsync("/api/public");
```

## Environment Variables

The test suite automatically loads `.env` file from the test output directory (or parent directories) before running tests.

This is handled by `EnvironmentModuleInitializer` which runs before any test code.

Example `.env` for tests:

```env
# Database - only needed if using custom factory for specific tests
DATABASE_CONNECTIONSTRING=Host=localhost;Port=5432;Database=test_db

# Authentication (if needed for specific test scenarios)
AUTHENTICATION__PROVIDERS__SUPABASE__AUTHORITY=https://test.supabase.co
AUTHENTICATION__PROVIDERS__SUPABASE__AUDIENCE=authenticated
AUTHENTICATION__PROVIDERS__SUPABASE__JWTSECRET=test-secret
```

## Database Testing

### Automatic PostgreSQL Container

Tests automatically spin up a PostgreSQL container using Testcontainers:

- **Image**: postgres:16
- **Username**: testuser
- **Password**: testpassword
- **Database**: postgres (default)

### Database Reset

Each test gets a clean database state:

1. Container starts once for all tests
2. Before each test, `ResetDatabasesAsync()` is called
3. Respawn truncates tables while preserving schema
4. Tests run with clean data
5. Container is disposed after all tests complete

## Running Tests

### All tests

```bash
dotnet test
```

### Specific test class

```bash
dotnet test --filter "FullyQualifiedName~MyIntegrationTests"
```

### With logging

```bash
dotnet test --verbosity detailed
```

## Best Practices

1. **Keep factories focused** - Each factory should manage one application type
2. **Use service customization sparingly** - Only when absolutely needed
3. **Dispose resources properly** - Use `IAsyncLifetime` for setup/teardown
4. **Use mocks for external services** - Keep tests fast and isolated
5. **Follow naming conventions** - `*IntegrationTests` suffix for test classes
6. **Clean test data** - Rely on automatic reset or explicit cleanup in test methods

## Dependencies

- **xunit** - Test framework
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework
- **Testcontainers** - Container management
- **Testcontainers.PostgreSql** - PostgreSQL container
- **Respawn** - Database cleanup
- **DotNetEnv** - Environment variable loading
- **Microsoft.AspNetCore.Mvc.Testing** - WebApplicationFactory support
