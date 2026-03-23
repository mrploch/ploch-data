# Generic Repository and Unit of Work

## Overview

The generic [Repository](https://martinfowler.com/eaaCatalog/repository.html) and [Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html) pattern implementation for .NET.

## Packages in this Directory

| Package | Description |
|---------|-------------|
| [Ploch.Data.GenericRepository](Data.GenericRepository/) | Provider-agnostic repository and UoW interfaces |
| [Ploch.Data.GenericRepository.EFCore](Data.GenericRepository.EFCore/) | EF Core implementations |
| [Ploch.Data.GenericRepository.EFCore.DependencyInjection](Data.GenericRepository.EFCore.DependencyInjection/) | ServicesBundle integration for Ploch.Common |
| [Ploch.Data.GenericRepository.EFCore.IntegrationTesting](Data.GenericRepository.EFCore.IntegrationTesting/) | Integration test base classes |
| [Ploch.Data.GenericRepository.EFCore.Specification](Data.GenericRepository.EFCore.Specification/) | Ardalis.Specification support |

## Motivation

While many consider EF Core a repository and unit of work, it lacks the ability to be easily mocked with simple interfaces. The Generic Repository pattern provides a testable abstraction that can also be backed by non-EF ORMs or NoSQL databases.

## Quick Start

```csharp
// Register all repositories + UnitOfWork for a DbContext
services.AddDbContext<MyDbContext>(options => options.UseSqlite(connectionString));
services.AddRepositories<MyDbContext>();

// Inject and use
public class MyService(IReadRepositoryAsync<MyEntity, int> repository)
{
    public Task<IList<MyEntity>> GetPageAsync(int page, int size)
        => repository.GetPageAsync(page, size);
}

// Multi-entity transactions with Unit of Work
public class MyTransactionService(IUnitOfWork unitOfWork)
{
    public async Task SaveAsync(MyEntity entity, OtherEntity other)
    {
        await unitOfWork.Repository<MyEntity, int>().AddAsync(entity);
        await unitOfWork.Repository<OtherEntity, int>().UpdateAsync(other);
        await unitOfWork.CommitAsync();
    }
}
```

## Documentation

See the [Generic Repository Guide](../../docs/generic-repository.md) for the full reference.
