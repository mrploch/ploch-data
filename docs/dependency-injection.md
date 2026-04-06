# Dependency Injection Guide

This guide covers how to register the Generic Repository, Unit of Work, and EF Core DbContext in a .NET dependency injection container. It explains three registration approaches, how to switch between database providers without changing application code, and how the `IDbContextCreationLifecycle` plugin system works.

## Quick Reference

| I want to... | Method | Package |
|--------------|--------|---------|
| Register everything in one call (SQLite) | `AddDbContextWithRepositories<TDbContext>()` | `Ploch.Data.GenericRepository.EFCore.SqLite` |
| Register everything in one call (SQL Server) | `AddDbContextWithRepositories<TDbContext>()` | `Ploch.Data.GenericRepository.EFCore.SqlServer` |
| Register repositories only (DbContext already registered) | `AddRepositories<TDbContext>()` | `Ploch.Data.GenericRepository.EFCore` |
| Register with a custom lifecycle | `AddDbContextWithRepositories<TDbContext, TLifecycle>(options)` | `Ploch.Data.GenericRepository.EFCore` |
| Register a custom domain-specific repository | `AddCustomReadWriteAsyncRepository<...>()` | `Ploch.Data.GenericRepository.EFCore` |

## Registration Approaches

### 1. Provider-Specific Packages (Recommended)

The simplest approach. A single call registers the DbContext, the correct lifecycle plugin, and all repository interfaces:

````csharp
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContextWithRepositories<MyAppDbContext>();
````

This method:
- Reads the connection string from `appsettings.json` (`ConnectionStrings:DefaultConnection`) automatically.
- Registers the correct `IDbContextCreationLifecycle` implementation for the chosen provider.
- Registers all repository interfaces (`IReadRepositoryAsync`, `IReadWriteRepositoryAsync`, etc.) as scoped services.
- Registers `IUnitOfWork` as a scoped service.
- Registers `IAuditEntityHandler` for automatic audit timestamp tracking.

**Providing a connection string explicitly:**

````csharp
builder.Services.AddDbContextWithRepositories<MyAppDbContext>(
    () => builder.Configuration.GetConnectionString("MyConnection"));
````

#### Required Packages

Choose **one** of:

| Package | Database | NuGet |
|---------|----------|-------|
| `Ploch.Data.GenericRepository.EFCore.SqLite` | SQLite | `Ploch.Data.GenericRepository.EFCore.SqLite` |
| `Ploch.Data.GenericRepository.EFCore.SqlServer` | SQL Server | `Ploch.Data.GenericRepository.EFCore.SqlServer` |

Both packages expose the **same namespace** (`Ploch.Data.GenericRepository.EFCore.DependencyInjection`), **same class name** (`ServiceCollectionRegistrations`), and **same method signature** (`AddDbContextWithRepositories<TDbContext>()`). This is by design -- see [Switching Database Providers](#switching-database-providers) below.

### 2. Manual Registration

When you need full control over DbContext options -- for example, when using a provider not covered by the provider-specific packages, or when you need to pass additional `DbContextOptionsBuilder` configuration:

````csharp
using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository.EFCore;

services.AddDbContext<MyAppDbContext>(options =>
    options.UseSqlite("Data Source=myapp.db"));

services.AddRepositories<MyAppDbContext>();
````

Or with a DbContext options callback:

````csharp
services.AddDbContextWithRepositories<MyAppDbContext>(options =>
    options.UseSqlite("Data Source=myapp.db"));
````

The overload accepting `Action<DbContextOptionsBuilder>` registers `DefaultDbContextCreationLifecycle` (no-op) by default. To specify a different lifecycle:

````csharp
services.AddDbContextWithRepositories<MyAppDbContext, SqLiteDbContextCreationLifecycle>(options =>
    options.UseSqlite("Data Source=myapp.db"));
````

## Switching Database Providers

The provider-specific DI packages are designed for **zero-code provider switching**. Both the SQLite and SQL Server packages share the same namespace and method signature. To switch providers:

### Step 1: Change the package reference

**SQLite:**

````xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.SqLite" />
````

**SQL Server:**

````xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.SqlServer" />
````

### Step 2: Update the connection string in appsettings.json

**SQLite:**

````json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=myapp.db;Cache=Shared"
  }
}
````

**SQL Server:**

````json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Integrated Security=True;TrustServerCertificate=True"
  }
}
````

### Step 3: There is no step 3

Your application code remains **identical**. The `using` directive, the `AddDbContextWithRepositories<TDbContext>()` call, and all repository/Unit of Work usage stay exactly the same.

### How It Works

Both packages define a `ServiceCollectionRegistrations` class in the `Ploch.Data.GenericRepository.EFCore.DependencyInjection` namespace. Since you only reference one package at a time, there is no ambiguity. The compiler resolves to whichever package is referenced.

Behind the scenes, each package:
1. Registers the appropriate `IDbContextCreationLifecycle` implementation (see [Lifecycle Plugins](#lifecycle-plugins-idbcontextcreationlifecycle)).
2. Configures the `DbContext` with the correct provider (`UseSqlite()` or `UseSqlServer()`).
3. Calls `AddRepositories<TDbContext>()` to register all repository interfaces and `IUnitOfWork`.

## Lifecycle Plugins (`IDbContextCreationLifecycle`)

The `IDbContextCreationLifecycle` interface allows database-specific logic to be injected into a `DbContext` without the `DbContext` itself referencing any specific provider.

### The Problem It Solves

Some database providers require special model configuration. For example, SQLite does not natively support `DateTimeOffset` in EF Core -- a value converter must be applied to all `DateTimeOffset` properties during `OnModelCreating`. Without the lifecycle plugin, the `DbContext` would need a direct reference to the SQLite package, making it provider-specific.

### The Interface

````csharp
public interface IDbContextCreationLifecycle
{
    void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database);
    void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
}
````

### Built-in Implementations

| Implementation | Provider | Behaviour |
|----------------|----------|-----------|
| `DefaultDbContextCreationLifecycle` | SQL Server / any | No-op -- no special configuration needed |
| `SqLiteDbContextCreationLifecycle` | SQLite | Applies `DateTimeOffset` → binary converter to all `DateTimeOffset` and `DateTimeOffset?` properties |

### Using the Lifecycle in Your DbContext (Optional, Recommended)

Accepting `IDbContextCreationLifecycle` in your `DbContext` constructor is **optional** -- your DbContext works fine without it. However, it is **recommended** if:

- Your application may target **multiple database providers** (e.g. SQLite for development, SQL Server for production).
- You want the Data project to remain **database-agnostic**, with provider-specific logic injected externally.
- You are building a **library or shared DbContext** that other applications will consume with different databases.

The parameter should use a **default value of `null`** so that the DbContext remains usable even when no lifecycle is registered in the DI container:

````csharp
public class MyAppDbContext : DbContext
{
    private readonly IDbContextCreationLifecycle? _lifecycle;

    public MyAppDbContext(
        DbContextOptions<MyAppDbContext> options,
        IDbContextCreationLifecycle? lifecycle = null) : base(options)
        => _lifecycle = lifecycle;

    protected MyAppDbContext(IDbContextCreationLifecycle? lifecycle = null)
        => _lifecycle = lifecycle;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyAppDbContext).Assembly);
        _lifecycle?.OnModelCreating(modelBuilder, Database);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        _lifecycle?.OnConfiguring(optionsBuilder);
    }
}
````

**Why `= null`?** The .NET DI container treats constructor parameters with default values as optional. If `IDbContextCreationLifecycle` is registered, the container injects it; if not, it passes `null`. Without the default value, the container would throw `InvalidOperationException` when resolving the DbContext. The `?.` null-conditional calls ensure the hooks are simply skipped when no lifecycle is present.

When using the provider-specific DI packages, the correct lifecycle implementation is registered automatically. You do not need to register it manually.

### Creating a Custom Lifecycle

To create a lifecycle for a database provider not covered by the built-in implementations:

````csharp
public class PostgresDbContextCreationLifecycle : IDbContextCreationLifecycle
{
    public void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database)
    {
        // Apply Postgres-specific model configuration
    }

    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Apply Postgres-specific options
    }
}

// Register manually
services.AddDbContextWithRepositories<MyAppDbContext, PostgresDbContextCreationLifecycle>(
    options => options.UseNpgsql(connectionString));
````

## What Gets Registered

When you call any of the registration methods, the following services are registered in the DI container:

| Service | Implementation | Lifetime |
|---------|---------------|----------|
| `IQueryableRepository<TEntity>` | `QueryableRepository<TEntity>` | Scoped |
| `IReadRepository<TEntity>` | `ReadRepository<TEntity>` | Scoped |
| `IReadRepositoryAsync<TEntity>` | `ReadRepositoryAsync<TEntity>` | Scoped |
| `IReadRepositoryAsync<TEntity, TId>` | `ReadRepositoryAsync<TEntity, TId>` | Scoped |
| `IWriteRepositoryAsync<TEntity, TId>` | `ReadWriteRepositoryAsync<TEntity, TId>` | Scoped |
| `IReadWriteRepositoryAsync<TEntity, TId>` | `ReadWriteRepositoryAsync<TEntity, TId>` | Scoped |
| `IUnitOfWork` | `UnitOfWork<TDbContext>` | Scoped |
| `IAuditEntityHandler` | `AuditEntityHandler` | Singleton |
| `IUserInfoProvider` | `NullUserInfoProvider` | Singleton |
| `IDbContextCreationLifecycle` | Provider-specific (see above) | Singleton |
| `DbContext` | Resolves `TDbContext` | Transient |

**Note:** `IUserInfoProvider` defaults to `NullUserInfoProvider` (no-op). To populate `CreatedBy` and `LastModifiedBy` on entities implementing `IHasAuditProperties`, register your own `IUserInfoProvider` implementation after the repository registration.

## Connection String Configuration

### Automatic (appsettings.json)

When using the provider-specific packages with no connection string argument, the connection string is loaded from `appsettings.json` via `ConnectionString.FromJsonFile()`:

````json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=myapp.db;Cache=Shared"
  }
}
````

The configuration file must be present in the application's output directory. Ensure it is copied during build:

````xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
````

### Explicit

Pass a function that returns the connection string:

````csharp
// From IConfiguration (e.g. in ASP.NET Core)
builder.Services.AddDbContextWithRepositories<MyAppDbContext>(
    () => builder.Configuration.GetConnectionString("DefaultConnection"));

// Hardcoded (e.g. in tests or console apps)
builder.Services.AddDbContextWithRepositories<MyAppDbContext>(
    () => "Data Source=:memory:");
````

### Custom Configuration File

Use a different JSON file or connection string name:

````csharp
builder.Services.AddDbContextWithRepositories<MyAppDbContext>(
    ConnectionString.FromJsonFile("config.json", "MyCustomConnection"));
````

## Custom Repository Registration

When you need domain-specific repository logic beyond what the generic repository provides, create a custom repository and register it:

````csharp
// 1. Define the interface
public interface IArticleRepository : IReadWriteRepositoryAsync<Article, int>
{
    Task<IList<Article>> GetPublishedAsync(CancellationToken ct = default);
}

// 2. Implement it
public class ArticleRepository(DbContext dbContext, IAuditEntityHandler auditHandler)
    : ReadWriteRepositoryAsync<Article, int>(dbContext, auditHandler),
      IArticleRepository
{
    public async Task<IList<Article>> GetPublishedAsync(CancellationToken ct = default)
    {
        return await Entities
            .Where(a => a.IsPublished)
            .Include(a => a.Author)
            .ToListAsync(ct);
    }
}

// 3. Register (replaces default registrations for Article)
services.AddCustomReadWriteAsyncRepository<
    IArticleRepository, ArticleRepository, Article, int>();
````

`AddCustomReadWriteAsyncRepository` registers the custom type for all repository interfaces for that entity type:
- `IArticleRepository`
- `IReadWriteRepositoryAsync<Article, int>`
- `IWriteRepositoryAsync<Article, int>`
- `IReadRepositoryAsync<Article, int>`
- `IReadRepositoryAsync<Article>`
- `IQueryableRepository<Article>`

## Integration Testing DI Setup

For integration tests, use the `GenericRepositoryDataIntegrationTest<TDbContext>` base class from `Ploch.Data.GenericRepository.EFCore.IntegrationTesting`. It automatically configures an in-memory SQLite database with a shared connection:

````csharp
public class ProductRepositoryTests
    : GenericRepositoryDataIntegrationTest<MyAppDbContext>
{
    [Fact]
    public async Task AddAsync_should_persist_product()
    {
        var repository = CreateReadWriteRepositoryAsync<Product, int>();

        await repository.AddAsync(new Product { Title = "Test" });
        await DbContext.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(1);
        saved.Should().NotBeNull();
    }
}
````

For manual test DI setup without the base class, use `DbContextServicesRegistrationHelper`:

````csharp
var services = new ServiceCollection();
services.AddSingleton<IDbContextCreationLifecycle, SqLiteDbContextCreationLifecycle>();

var (provider, dbContext) = DbContextServicesRegistrationHelper
    .BuildDbContextAndServiceProvider<MyAppDbContext>(services);

await dbContext.Database.EnsureCreatedAsync();

var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
````

## Complete Examples

### ASP.NET Core Web API with SQLite

````csharp
// Program.cs
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextWithRepositories<MyAppDbContext>();

var app = builder.Build();

app.MapGet("/products/{id}", async (int id,
    IReadRepositoryAsync<Product, int> repo) =>
{
    var product = await repo.GetByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.Run();
````

### Console Application with SQL Server

````csharp
// Program.cs
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContextWithRepositories<MyAppDbContext>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
var repo = unitOfWork.Repository<Product, int>();

await repo.AddAsync(new Product { Title = "Widget" });
await unitOfWork.CommitAsync();
````

### Switching the above from SQL Server to SQLite

1. In the `.csproj`, change:
   ````xml
   <!-- Before -->
   <PackageReference Include="Ploch.Data.GenericRepository.EFCore.SqlServer" />
   <!-- After -->
   <PackageReference Include="Ploch.Data.GenericRepository.EFCore.SqLite" />
   ````

2. In `appsettings.json`, change:
   ````json
   {
     "ConnectionStrings": {
       "DefaultConnection": "DataSource=myapp.db;Cache=Shared"
     }
   }
   ````

3. `Program.cs` remains **unchanged**.

## See Also

- [Getting Started](getting-started.md) -- quick start for common use cases
- [Generic Repository Guide](generic-repository.md) -- full API reference for repository operations
- [Data Project Setup](data-project-setup.md) -- creating Data and provider projects with migrations
- [Integration Testing](integration-testing.md) -- testing patterns and base classes
- [Sample Application](../samples/SampleApp/) -- complete working example demonstrating all features
