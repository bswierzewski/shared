# Shared.Users.Tests.EndToEnd

Testy end-to-end dla modułu Users. Wykorzystuje infrastrukturę testową z `Shared.Infrastructure.Tests`.

## Podejścia do testów

### 1. Współdzielony TestContext (użyte w tym projekcie)

**Kiedy używać:** Testy nie potrzebują mocków - wszystkie serwisy są prawdziwe.

**Zalety:**
- Prostszy kod
- Szybsze testy (jeden host dla wszystkich testów)
- Współdzielony cache tokenów

**Struktura:**

```csharp
// Fixture - tworzy kontener i TestContext
public class UsersTestFixture : IAsyncLifetime
{
    public PostgreSqlTestContainer Container { get; } = new();
    public TestContext Context { get; private set; } = null!;
    public TestUserOptions TestUser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        Context = await TestContext.CreateBuilder<Program>()
            .WithContainer(Container)
            .WithServices((services, configuration) =>
            {
                services.ConfigureOptions<TestUserOptions>(configuration);
                services.AddSingleton<ITokenProvider, SupabaseTokenProvider>();
            })
            .BuildAsync();

        TestUser = Context.GetRequiredService<IOptions<TestUserOptions>>().Value;
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await Container.StopAsync();
    }
}

[CollectionDefinition("Users")]
public class UsersCollection : ICollectionFixture<UsersTestFixture> { }
```

```csharp
// Test - używa współdzielonego kontekstu
[Collection("Users")]
public class UserEndpointsTests(UsersTestFixture fixture) : IAsyncLifetime
{
    private readonly TestContext _context = fixture.Context;

    public async Task InitializeAsync()
    {
        var token = await _context.GetTokenAsync(fixture.TestUser.Email, fixture.TestUser.Password);
        _context.Client.WithBearerToken(token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetUser_ReturnsOk()
    {
        await _context.ResetDatabaseAsync();
        
        var response = await _context.Client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

### 2. Per-class TestContext z mockami

**Kiedy używać:** Testy potrzebują mocków - każda klasa ma inne zależności.

**Zalety:**
- Pełna kontrola nad mockami per klasa
- Izolacja konfiguracji serwisów
- Możliwość nadpisywania zachowań mocków w testach

**Struktura:**

```csharp
// Fixture - kontener, token provider (bez współdzielonego TestContext!)
public class MySharedFixture : IAsyncLifetime
{
    private TestContext _bootstrapContext = null!;

    public PostgreSqlTestContainer Container { get; } = new();
    public ITokenProvider TokenProvider { get; private set; } = null!;
    public TestUserOptions TestUser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        // Bootstrap context - do migracji i pobrania konfiguracji
        _bootstrapContext = await TestContext.CreateBuilder<Program>()
            .WithContainer(Container)
            .WithServices((services, configuration) =>
            {
                services.ConfigureOptions<TestUserOptions>(configuration);
                services.AddSingleton<ITokenProvider, SupabaseTokenProvider>();
            })
            .BuildAsync();

        TestUser = _bootstrapContext.GetRequiredService<IOptions<TestUserOptions>>().Value;
        TokenProvider = _bootstrapContext.GetRequiredService<ITokenProvider>();
    }

    public async Task DisposeAsync()
    {
        await _bootstrapContext.DisposeAsync();
        await Container.StopAsync();
    }
}

[CollectionDefinition("MyModule")]
public class MyModuleCollection : ICollectionFixture<MySharedFixture> { }
```

```csharp
// Test - tworzy WŁASNY TestContext z mockami
[Collection("MyModule")]
public class InvoicingTests(MySharedFixture shared) : IAsyncLifetime
{
    private TestContext _context = null!;
    
    // Mocki jako pola klasy
    private readonly Mock<IContractorsAPI> _mockContractorsApi = new();
    private readonly Mock<IOrdersAPI> _mockOrdersApi = new();

    public async Task InitializeAsync()
    {
        // Konfiguracja mocków
        _mockContractorsApi.Setup(x => x.GetContractorByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContractorDto(...));

        // Własny TestContext z mockami
        _context = await TestContext.CreateBuilder<Program>()
            .WithContainer(shared.Container)  // Współdzielony kontener
            .WithServices((services, _) =>
            {
                // Wstrzyknięcie mocków
                services.ReplaceInstance(_mockContractorsApi.Object);
                services.ReplaceInstance(_mockOrdersApi.Object);
            })
            .BuildAsync();

        // Token z współdzielonego providera (cache)
        var token = await shared.TokenProvider.GetTokenAsync(shared.TestUser.Email, shared.TestUser.Password);
        _context.Client.WithBearerToken(token);
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task CreateInvoice_WhenContractorNotFound_ReturnsError()
    {
        await _context.ResetDatabaseAsync();

        // Nadpisanie mocka dla tego testu
        var unknownId = Guid.NewGuid();
        _mockContractorsApi.Setup(x => x.GetContractorByIdAsync(unknownId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContractorDto?)null);

        var response = await _context.Client.PostJsonAsync("/api/invoicing/create", new { ContractorId = unknownId });
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

---

### 3. Wiele kolekcji z osobnymi kontenerami

**Kiedy używać:** Potrzebujesz izolacji na poziomie bazy danych między grupami testów.

**Struktura:**

```csharp
// Kolekcja 1 - Users (własny kontener)
public class UsersFixture : IAsyncLifetime
{
    public PostgreSqlTestContainer Container { get; } = new();
    public TestContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
        Context = await TestContext.CreateBuilder<Program>()
            .WithContainer(Container)
            .BuildAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await Container.StopAsync();
    }
}

[CollectionDefinition("Users")]
public class UsersCollection : ICollectionFixture<UsersFixture> { }

// Kolekcja 2 - Invoicing (OSOBNY kontener!)
public class InvoicingFixture : IAsyncLifetime
{
    public PostgreSqlTestContainer Container { get; } = new();
    public TestContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
        Context = await TestContext.CreateBuilder<Program>()
            .WithContainer(Container)
            .BuildAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await Container.StopAsync();
    }
}

[CollectionDefinition("Invoicing")]
public class InvoicingCollection : ICollectionFixture<InvoicingFixture> { }
```

---

## Porównanie podejść

| Aspekt | Współdzielony TestContext | Per-class TestContext | Wiele kolekcji |
|--------|---------------------------|----------------------|----------------|
| Mocki | ❌ Nie | ✅ Tak | ✅ Tak |
| Szybkość | ✅ Najszybsze | ⚠️ Wolniejsze | ⚠️ Zależy od liczby kolekcji |
| Izolacja serwisów | ❌ Brak | ✅ Per klasa | ✅ Per klasa |
| Izolacja bazy | ❌ Wspólna baza | ❌ Wspólna baza | ✅ Osobna baza per kolekcja |
| Prostota | ✅ Najprostsze | ⚠️ Więcej kodu | ⚠️ Więcej kodu |
| Kontener PostgreSQL | 1× | 1× | N× (per kolekcja) |
| WebApplicationFactory | 1× | N× (per klasa) | N× (per klasa) |

---

## Przepływ wykonania

```
Kolekcja testów "Users"
│
├── UsersTestFixture.InitializeAsync()           [RAZ]
│   ├── Container.StartAsync()                   ~5-10s
│   ├── TestContext.CreateBuilder().BuildAsync() ~2s
│   └── Migracje EF Core                         ~1s
│
├── UserEndpointsTests                           [Klasa 1]
│   ├── Test1: ResetDatabase + HTTP Request
│   ├── Test2: ResetDatabase + HTTP Request
│   └── ...
│
├── UserProvisioningTests                        [Klasa 2]
│   ├── Test1: ResetDatabase + HTTP Request
│   └── ...
│
└── UsersTestFixture.DisposeAsync()              [RAZ]
    ├── Context.DisposeAsync()
    └── Container.StopAsync()
```

---

## Uruchamianie testów

```bash
# Wszystkie testy
dotnet test

# Tylko testy Users
dotnet test --filter "FullyQualifiedName~Shared.Users.Tests"

# Konkretny test
dotnet test --filter "FullyQualifiedName~GetUserById_WithValidUser"
```
