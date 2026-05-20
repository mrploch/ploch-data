# Ploch.Data.GenericRepository.EFCore

Entity Framework Core implementations of the Generic Repository and Unit of Work patterns defined in `Ploch.Data.GenericRepository`.

## Key Features

- **Full repository implementations** -- `ReadRepositoryAsync<TEntity, TId>`, `ReadWriteRepositoryAsync<TEntity, TId>`, `QueryableRepository<TEntity>`
- **Unit of Work** -- `UnitOfWork<TDbContext>` with `CommitAsync` and `RollbackAsync`
- **DI registration** -- `AddRepositories<TDbContext>()` registers all interfaces with a single call
- **Custom repository support** -- `AddCustomReadWriteAsyncRepository` for domain-specific repositories
- **Audit entity handler** -- `AuditEntityHandler` for automatic timestamp and user tracking
- **Eager loading** -- `onDbSet` parameter for `Include`/`ThenInclude` on read operations

## Installation

```xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore" />
```

## Quick Start

For most applications, use one of the provider-specific packages instead of this package directly:
- `Ploch.Data.GenericRepository.EFCore.SqLite` -- SQLite
- `Ploch.Data.GenericRepository.EFCore.SqlServer` -- SQL Server

```csharp
// Recommended: use a provider-specific package for one-call registration
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;
builder.Services.AddDbContextWithRepositories<MyDbContext>();

// Alternative: manual registration with this package
services.AddDbContext<MyDbContext>(options => options.UseSqlite(connectionString));
services.AddRepositories<MyDbContext>();
```

```csharp
// Use repositories
public class MyService(IReadWriteRepositoryAsync<Product, int> repo)
{
    public async Task CreateAsync(Product product)
    {
        await repo.AddAsync(product);
    }

    public async Task<Product?> GetWithDetailsAsync(int id)
    {
        return await repo.GetByIdAsync(id,
            onDbSet: q => q.Include(p => p.Category).Include(p => p.Tags));
    }
}
```

## Documentation

- [Dependency Injection Guide](../../../docs/dependency-injection.md) -- all DI registration approaches, provider switching, and lifecycle plugins
- [Generic Repository Guide](../../../docs/generic-repository.md) -- all read/write operations, Unit of Work patterns, specifications, and error handling
