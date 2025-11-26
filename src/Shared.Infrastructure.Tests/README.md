# Shared.Infrastructure.Tests 2.0

A comprehensive testing infrastructure library for integration and end-to-end tests in modular monolith applications. Provides a clean, fluent API for setting up test environments with PostgreSQL containers, automatic database cleanup, and service customization.

## Quick Start

```csharp
[Collection("Integration")]
public class UserEndpointsTests : IAsyncLifetime
{
    private TestContext _context = null!;

    public async Task InitializeAsync()
    {
        _context = await TestContext.CreateBuilder<Program>()
            .WithPostgreSql()
            .BuildAsync();
    }

    public Task DisposeAsync() => _context.DisposeAsync().AsTask();

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        await _context.ResetDatabaseAsync();

        var response = await _context.Client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Table of Contents

- [Installation](#installation)
- [Architecture](#architecture)
- [Core Concepts](#core-concepts)
- [Usage Patterns](#usage-patterns)
- [Extension Methods](#extension-methods)
- [Best Practices](#best-practices)
- [Migration from 1.x](#migration-from-1x)

## Installation

```xml
<PackageReference Include="Shared.Infrastructure.Tests" Version="2.0.0" />
```

## Architecture

```
Shared.Infrastructure.Tests/
├── Core/
│   ├── TestContext.cs              # Main entry point for tests
│   ├── ITestHost.cs                # Test host abstraction
│   └── TestHost.cs                 # WebApplicationFactory-based implementation
├── Builders/
│   ├── TestContextBuilder.cs       # Fluent builder for test setup
│   └── TestHostBuilder.cs          # Internal host builder
├── Infrastructure/
│   ├── Containers/
│   │   ├── ITestContainer.cs       # Container abstraction
│   │   └── PostgreSqlTestContainer.cs
│   ├── Database/
│   │   ├── DatabaseResetStrategy.cs
│   │   ├── IDatabaseManager.cs
│   │   └── DatabaseManager.cs
│   └── Environment/
│       └── EnvironmentLoader.cs     # Auto-loads .env files
├── Extensions/
│   ├── Http/
│   │   └── HttpClientExtensions.cs  # POST/GET/PUT JSON helpers
│   ├── Services/
│   │   └── ServiceCollectionExtensions.cs  # ReplaceMock, ReplaceService
│   └── Database/
│       └── DbContextExtensions.cs   # ReplaceDbContext
└── Fixtures/
    ├── ITestHostFixture.cs
    └── TestHostFixture.cs           # Shared fixture support
```

## Core Concepts

See [README.md](./README.md) for complete documentation.

## Migration from 1.x

See [MIGRATION_GUIDE.md](./MIGRATION_GUIDE.md) for detailed migration instructions.

---

**Version**: 2.0.0
