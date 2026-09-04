# Generic Repository and Unit of Work

## Overview

The generic [Repository](https://martinfowler.com/eaaCatalog/repository.html) and [Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html) pattern implementation for .NET.

## Packages in this Directory

| Package | Description |
|---------|-------------|
| [Ploch.Data.GenericRepository](https://github.com/mrploch/ploch-data/tree/main/src/Data.GenericRepository/Data.GenericRepository/) | Provider-agnostic repository and UoW interfaces |
| [Ploch.Data.GenericRepository.EFCore](https://github.com/mrploch/ploch-data/tree/main/src/Data.GenericRepository/Data.GenericRepository.EFCore/) | EF Core implementations and manual DI registration (`AddRepositories`) |
| [Ploch.Data.GenericRepository.EFCore.SqLite](https://github.com/mrploch/ploch-data/tree/main/src/Data.GenericRepository/Data.GenericRepository.EFCore.SqLite/) | One-call DI registration for SQLite (`AddDbContextWithRepositories`) |
| [Ploch.Data.GenericRepository.EFCore.SqlServer](https://github.com/mrploch/ploch-data/tree/main/src/Data.GenericRepository/Data.GenericRepository.EFCore.SqlServer/) | One-call DI registration for SQL Server (`AddDbContextWithRepositories`) |
| [Ploch.Data.GenericRepository.EFCore.IntegrationTesting](https://github.com/mrploch/ploch-data/tree/main/src/Data.GenericRepository/Data.GenericRepository.EFCore.IntegrationTesting/) | Integration test base classes |
| [Ploch.Data.GenericRepository.EFCore.Specification](https://github.com/mrploch/ploch-data/tree/main/src/Data.GenericRepository/Data.GenericRepository.EFCore.Specification/) | Ardalis.Specification support |

## Motivation

While many consider EF Core a repository and unit of work, it lacks the ability to be easily mocked with simple interfaces. The Generic Repository pattern provides a testable abstraction that can also be backed by non-EF ORMs or NoSQL databases.

## Quick Start

```csharp
// Register DbContext + all repositories + UnitOfWork in one call
// Reference Ploch.Data.GenericRepository.EFCore.SqLite or .SqlServer
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

builder.Services.AddDbContextWithRepositories<MyDbContext>();

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

To switch between SQLite and SQL Server, change the package reference and update `appsettings.json` -- no code changes needed. See the [Dependency Injection Guide](https://github.com/mrploch/ploch-data/blob/main/docs/dependency-injection.md) for details.

## Documentation

See the [Generic Repository Guide](https://github.com/mrploch/ploch-data/blob/main/docs/generic-repository.md) for the full reference.
