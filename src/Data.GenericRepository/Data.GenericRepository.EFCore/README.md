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

```csharp
// Register in DI
services.AddDbContext<MyDbContext>(options => options.UseSqlite(connectionString));
services.AddRepositories<MyDbContext>();

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

See the [Generic Repository Guide](../../../docs/generic-repository.md) for detailed documentation including all read/write operations, Unit of Work patterns, specifications, and error handling.
