# Ploch.Data.GenericRepository

Provider-agnostic interfaces for the Generic Repository and Unit of Work patterns.

## Key Features

- **Repository interface hierarchy** -- `IQueryableRepository<TEntity>`, `IReadRepositoryAsync<TEntity>`, `IReadRepositoryAsync<TEntity, TId>`, `IReadWriteRepositoryAsync<TEntity, TId>`
- **Unit of Work** -- `IUnitOfWork` for atomic multi-entity transactions
- **Async-first** -- all operations return `Task` for async/await usage
- **Write interfaces** -- `IWriteRepositoryAsync<TEntity, TId>` for add, update, delete
- **Audit handling** -- `IAuditEntityHandler` for automatic audit property population
- **Structured exceptions** -- `DataAccessException`, `DataUpdateException`, `DataUpdateConcurrencyException`, `EntityNotFoundException`

## Installation

```xml
<PackageReference Include="Ploch.Data.GenericRepository" />
```

For EF Core implementations, use `Ploch.Data.GenericRepository.EFCore` instead (it includes this package).

## Quick Start

```csharp
// Inject the most restrictive interface needed
public class ProductService(IReadRepositoryAsync<Product, int> repository)
{
    public Task<Product?> GetAsync(int id) => repository.GetByIdAsync(id);

    public Task<IList<Product>> SearchAsync(string term)
        => repository.GetAllAsync(p => p.Title.Contains(term));
}

// Use IUnitOfWork for multi-entity transactions
public class OrderService(IUnitOfWork unitOfWork)
{
    public async Task PlaceOrderAsync(Order order)
    {
        var repo = unitOfWork.Repository<Order, int>();
        await repo.AddAsync(order);
        await unitOfWork.CommitAsync();
    }
}
```

## Documentation

See the [Generic Repository Guide](https://github.com/mrploch/ploch-data/blob/main/docs/generic-repository.md) for the complete reference including DI registration, eager loading, specifications, and error handling.
