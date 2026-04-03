# Getting Started

This guide covers the most common scenarios for using Ploch.Data libraries. Each section is self-contained -- jump directly to the one that matches your use case.

## Prerequisites

- .NET 9.0 SDK or later
- A NuGet source configured for `nuget.pkg.github.com/mrploch` (GitHub Packages) or local project references

## 1. Using Ploch.Data.Model (Entity Interfaces Only)

If you only need the standardised entity interfaces -- for example, to share a common model shape across projects or to enable generic UI components -- reference `Ploch.Data.Model`.

### Define your entities

````csharp
using Ploch.Data.Model;
using Ploch.Data.Model.CommonTypes;

public class Product : IHasId<int>, IHasTitle, IHasDescription, IHasAuditTimeProperties
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public DateTimeOffset? AccessedTime { get; set; }
}

// A tag type for products
public class ProductTag : Tag<int>
{ }

// A hierarchical category for products
public class ProductCategory : Category<ProductCategory>
{ }
````

### Benefits

- All entities share a consistent property shape (`Id`, `Title`, `CreatedTime`, etc.).
- Generic components (UI, API endpoints, repositories) can operate on any entity implementing these interfaces.
- Audit timestamp handling can be centralised in the DbContext.

## 2. Using Ploch.Data.EFCore (DbContext Factories)

The `Ploch.Data.EFCore` package provides base classes for design-time DbContext factories, which are required for EF Core migrations.

### Create a DbContext

````csharp
using Microsoft.EntityFrameworkCore;

public class MyAppDbContext : DbContext
{
    protected MyAppDbContext() { }

    public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyAppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
````

### Create a SQLite design-time factory

````csharp
using Ploch.Data.EFCore.SqLite;

public class MyAppDbContextFactory()
    : SqLiteDbContextFactory<MyAppDbContext, MyAppDbContextFactory>(
        options => new MyAppDbContext(options));
````

This single line provides everything EF Core tooling needs to run `dotnet ef migrations add` and `dotnet ef database update`.

### Create a SQL Server design-time factory

````csharp
using Ploch.Data.EFCore.SqlServer;

public class MyAppDbContextFactory()
    : SqlServerDbContextFactory<MyAppDbContext, MyAppDbContextFactory>(
        options => new MyAppDbContext(options));
````

Both factories read the connection string from `appsettings.json` in the output directory by default.

## 3. Using Generic Repository with EF Core

The Generic Repository provides a clean abstraction over EF Core for CRUD operations, pagination, and filtering.

### Install

Reference **one** of the provider-specific DI packages (or project references for local development):

| Package | Database |
|---------|----------|
| `Ploch.Data.GenericRepository.EFCore.SqLite` | SQLite |
| `Ploch.Data.GenericRepository.EFCore.SqlServer` | SQL Server |

Both packages also bring in `Ploch.Data.GenericRepository.EFCore` and `Ploch.Data.Model` transitively.

### Register in DI

````csharp
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContextWithRepositories<MyAppDbContext>();
````

This single call registers the DbContext with the correct database provider, all repository interfaces (`IReadRepositoryAsync`, `IReadWriteRepositoryAsync`, etc.), `IUnitOfWork`, and the appropriate `IDbContextCreationLifecycle` implementation. The connection string is loaded from `appsettings.json` (`ConnectionStrings:DefaultConnection`) automatically.

**Switching providers** requires only changing the package reference and the connection string in `appsettings.json` -- no code changes. See the [Dependency Injection Guide](dependency-injection.md) for details.

Alternatively, if you need full control over DbContext options, register manually:

````csharp
services.AddDbContext<MyAppDbContext>(
    options => options.UseSqlite("Data Source=myapp.db"));
services.AddRepositories<MyAppDbContext>();
````

### Inject and use a repository

````csharp
using Ploch.Data.GenericRepository;

public class ProductService(IReadRepositoryAsync<Product, int> productRepository)
{
    public async Task<Product?> GetProductAsync(int id)
    {
        return await productRepository.GetByIdAsync(id);
    }

    public async Task<IList<Product>> SearchAsync(string titleContains)
    {
        return await productRepository.GetAllAsync(
            p => p.Title.Contains(titleContains));
    }

    public async Task<IList<Product>> GetPageAsync(int page, int pageSize)
    {
        return await productRepository.GetPageAsync(page, pageSize,
            sortBy: p => p.Title);
    }
}
````

## 4. Using Unit of Work

When you need to modify multiple entity types in a single atomic transaction, use `IUnitOfWork`.

````csharp
using Ploch.Data.GenericRepository;

public class OrderService(IUnitOfWork unitOfWork)
{
    public async Task PlaceOrderAsync(Order order, IEnumerable<OrderItem> items)
    {
        var orderRepo = unitOfWork.Repository<Order, int>();
        var itemRepo = unitOfWork.Repository<OrderItem, int>();

        await orderRepo.AddAsync(order);

        foreach (var item in items)
        {
            item.Order = order;
            await itemRepo.AddAsync(item);
        }

        // All changes are committed atomically
        await unitOfWork.CommitAsync();
    }
}
````

### When to use Unit of Work vs direct repository injection

| Scenario | Use |
|----------|-----|
| Single entity type, read-only | `IReadRepositoryAsync<TEntity, TId>` |
| Single entity type, read + write | `IReadWriteRepositoryAsync<TEntity, TId>` |
| Multiple entity types, atomic transaction | `IUnitOfWork` |

## 5. Setting Up Integration Testing

The integration testing packages provide base classes that automatically configure an in-memory SQLite database.

### Install

Reference `Ploch.Data.GenericRepository.EFCore.IntegrationTesting`.

### Write a test

````csharp
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;

public class ProductRepositoryTests
    : GenericRepositoryDataIntegrationTest<MyAppDbContext>
{
    [Fact]
    public async Task AddAsync_should_persist_product()
    {
        var repository = CreateReadWriteRepositoryAsync<Product, int>();

        var product = new Product
        {
            Title = "Test Product",
            Description = "A test product"
        };

        await repository.AddAsync(product);
        await DbContext.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(product.Id);

        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Product");
    }

    [Fact]
    public async Task CommitAsync_should_persist_via_unit_of_work()
    {
        var unitOfWork = CreateUnitOfWork();
        var repo = unitOfWork.Repository<Product, int>();

        await repo.AddAsync(new Product { Title = "UoW Product" });
        await unitOfWork.CommitAsync();

        var all = await repo.GetAllAsync();
        all.Should().ContainSingle();
    }
}
````

The `GenericRepositoryDataIntegrationTest<TDbContext>` base class provides:

- `DbContext` -- the configured EF Core context backed by in-memory SQLite.
- `CreateUnitOfWork()` -- creates a new `IUnitOfWork` instance.
- `CreateReadRepositoryAsync<TEntity, TId>()` -- creates a typed read repository.
- `CreateReadWriteRepositoryAsync<TEntity, TId>()` -- creates a typed read/write repository.

## Next Steps

- [Data Model Guide](data-model.md) -- learn about all available interfaces and common types.
- [Generic Repository Guide](generic-repository.md) -- deep dive into repository operations, specifications, and error handling.
- [Data Project Setup](data-project-setup.md) -- create a full data layer with provider projects and migrations.
- [Sample Application](../samples/SampleApp/) -- explore a complete working example.
