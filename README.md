[![Build, Test and Analyze .NET](https://github.com/mrploch/ploch-data/actions/workflows/build-dotnet.yml/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/build-dotnet.yml)
[![pages-build-deployment](https://github.com/mrploch/ploch-data/actions/workflows/pages/pages-build-deployment/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/pages/pages-build-deployment)
[![Qodana](https://github.com/mrploch/ploch-data/actions/workflows/code_quality.yml/badge.svg)](https://github.com/mrploch/ploch-data/actions/workflows/code_quality.yml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-data&metric=alert_status&token=1ea9277b2f110b6b2d99685a20c037074d08d1c1)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-data)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mrploch_ploch-data&metric=coverage&token=1ea9277b2f110b6b2d99685a20c037074d08d1c1)](https://sonarcloud.io/summary/new_code?id=mrploch_ploch-data)

| Package | Version | Downloads |
|---------|---------|-----------|
| Ploch.Data.Model | [![version][model-v]][model] | [![downloads][model-d]][model] |
| Ploch.Data.GenericRepository | [![version][gr-v]][gr] | [![downloads][gr-d]][gr] |
| Ploch.Data.GenericRepository.EFCore | [![version][gref-v]][gref] | [![downloads][gref-d]][gref] |
| Ploch.Data.EFCore | [![version][ef-v]][ef] | [![downloads][ef-d]][ef] |

[model]: https://www.nuget.org/packages/Ploch.Data.Model/
[model-v]: https://img.shields.io/nuget/v/Ploch.Data.Model.svg
[model-d]: https://img.shields.io/nuget/dt/Ploch.Data.Model.svg
[gr]: https://www.nuget.org/packages/Ploch.Data.GenericRepository/
[gr-v]: https://img.shields.io/nuget/v/Ploch.Data.GenericRepository.svg
[gr-d]: https://img.shields.io/nuget/dt/Ploch.Data.GenericRepository.svg
[gref]: https://www.nuget.org/packages/Ploch.Data.GenericRepository.EFCore/
[gref-v]: https://img.shields.io/nuget/v/Ploch.Data.GenericRepository.EFCore.svg
[gref-d]: https://img.shields.io/nuget/dt/Ploch.Data.GenericRepository.EFCore.svg
[ef]: https://www.nuget.org/packages/Ploch.Data.EFCore/
[ef-v]: https://img.shields.io/nuget/v/Ploch.Data.EFCore.svg
[ef-d]: https://img.shields.io/nuget/dt/Ploch.Data.EFCore.svg

# Ploch.Data Libraries

A set of .NET libraries for building data access layers using standardised entity models, the Generic Repository and Unit of Work patterns, and Entity Framework Core.

## Quick Start

```csharp
// 1. Define entities using Ploch.Data.Model interfaces
public class Product : IHasId<int>, IHasTitle, IHasDescription, IHasAuditTimeProperties
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public DateTimeOffset? AccessedTime { get; set; }
}

// 2. Register in DI (reference Ploch.Data.GenericRepository.EFCore.SqLite
//    or Ploch.Data.GenericRepository.EFCore.SqlServer — same code for both)
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;
builder.Services.AddDbContextWithRepositories<MyDbContext>();

// 3. Inject and use repositories
public class ProductService(IReadWriteRepositoryAsync<Product, int> repository)
{
    public Task<Product?> GetAsync(int id) => repository.GetByIdAsync(id);
    public Task<IList<Product>> SearchAsync(string term)
        => repository.GetAllAsync(p => p.Title.Contains(term));
}
```

## Documentation

Full documentation is available in the [docs/](docs/) folder:

- [Getting Started](docs/getting-started.md) -- quick start guides for common use cases
- [Data Model Guide](docs/data-model.md) -- complete reference for `Ploch.Data.Model` interfaces
- [Generic Repository Guide](docs/generic-repository.md) -- repository operations, Unit of Work, specifications
- [Dependency Injection Guide](docs/dependency-injection.md) -- DI registration, provider switching (SQLite/SQL Server), lifecycle plugins, connection string configuration
- [Data Project Setup](docs/data-project-setup.md) -- step-by-step guide for creating Data and provider projects
- [Integration Testing](docs/integration-testing.md) -- testing with in-memory SQLite, base test classes
- [Extending the Libraries](docs/extending.md) -- custom repositories, new providers, extensibility
- [Architecture Overview](docs/architecture.md) -- package dependencies and design

## Sample Application

A fully working [Sample Application](samples/SampleApp/) demonstrates entity modelling, repository operations, Unit of Work, pagination, eager loading, and integration testing.

## Packages

### Core

| Package | Description |
|---------|-------------|
| [Ploch.Data.Model](src/Data.Model/) | Standardised entity interfaces (`IHasId`, `INamed`, `IHasTitle`, `IHasAuditProperties`, etc.) and common base types (`Category`, `Tag`, `Property`) |

### Entity Framework Core

| Package | Description |
|---------|-------------|
| [Ploch.Data.EFCore](src/Data.EFCore/) | Design-time factory base classes, `IDbContextConfigurator`, data seeding, value converters |
| [Ploch.Data.EFCore.SqLite](src/Data.EFCore.SqLite/) | SQLite provider: factory, configurator, `DateTimeOffset` workaround |
| [Ploch.Data.EFCore.SqlServer](src/Data.EFCore.SqlServer/) | SQL Server provider: factory, configurator |

### Generic Repository

| Package | Description |
|---------|-------------|
| [Ploch.Data.GenericRepository](src/Data.GenericRepository/Data.GenericRepository/) | Provider-agnostic repository and Unit of Work interfaces |
| [Ploch.Data.GenericRepository.EFCore](src/Data.GenericRepository/Data.GenericRepository.EFCore/) | EF Core implementations, DI registration via `AddRepositories<TDbContext>()` |
| [Ploch.Data.GenericRepository.EFCore.SqLite](src/Data.GenericRepository/Data.GenericRepository.EFCore.SqLite/) | One-call DI registration for SQLite (`AddDbContextWithRepositories<TDbContext>()`) |
| [Ploch.Data.GenericRepository.EFCore.SqlServer](src/Data.GenericRepository/Data.GenericRepository.EFCore.SqlServer/) | One-call DI registration for SQL Server (`AddDbContextWithRepositories<TDbContext>()`) |
| [Ploch.Data.GenericRepository.EFCore.Specification](src/Data.GenericRepository/Data.GenericRepository.EFCore.Specification/) | Ardalis.Specification integration |

### Testing

| Package | Description |
|---------|-------------|
| [Ploch.Data.EFCore.IntegrationTesting](src/Data.EFCore.IntegrationTesting/) | `DataIntegrationTest<TDbContext>` base class for EF Core tests |
| [Ploch.Data.GenericRepository.EFCore.IntegrationTesting](src/Data.GenericRepository/Data.GenericRepository.EFCore.IntegrationTesting/) | `GenericRepositoryDataIntegrationTest<TDbContext>` with repository/UoW helpers |

### Utilities

| Package | Description |
|---------|-------------|
| [Ploch.Data.Utilities](src/Data.Utilities/) | Various utility types for working with data |
| [Ploch.Data.StandardDataSets](src/Data.StandardDataSets/) | Common datasets (country lists, regions, etc.) |

## Features

### [Common Data Model](src/Data.Model/)

A set of interfaces and base types for standardising entity models:

- Core interfaces: `IHasId<TId>`, `INamed`, `IHasTitle`, `IHasDescription`, `IHasContents`, `IHasNotes`, `IHasValue<TValue>`
- Audit interfaces: `IHasAuditProperties`, `IHasAuditTimeProperties` (and individual timestamp/user interfaces)
- Hierarchical interfaces: `IHierarchicalParentChildrenComposite<T>` for tree structures
- Categorisation: `IHasCategories<TCategory>`, `IHasTags<TTag>`
- Common types: `Category<T>`, `Tag`, `Property<TValue>`, `StringProperty`, `IntProperty`, `Image`

### [Generic Repository and Unit of Work](src/Data.GenericRepository/)

A generic repository and unit of work pattern implementation for Entity Framework Core:

- Layered repository interfaces: `IQueryableRepository`, `IReadRepositoryAsync`, `IReadWriteRepositoryAsync`
- Unit of Work: `IUnitOfWork` with `CommitAsync()` and `RollbackAsync()`
- One-line DI registration: `services.AddDbContextWithRepositories<MyDbContext>()` (provider-specific) or `services.AddRepositories<MyDbContext>()` (manual)
- Zero-code database provider switching between SQLite and SQL Server
- Pluggable `IDbContextCreationLifecycle` for provider-specific model configuration
- Custom repository support with full interface registration
- Specification pattern integration via Ardalis.Specification
- Automatic audit property handling

### [Entity Framework Core Utilities](src/Data.EFCore/)

Utility classes for EF Core including design-time DbContext factory base classes, runtime configurators for SQLite and SQL Server, and the SQLite `DateTimeOffset` workaround.

### [Integration Testing](src/Data.EFCore.IntegrationTesting/)

Base classes for integration tests using in-memory SQLite databases with automatic schema creation, repository helpers, and Unit of Work support.

### [Common Datasets](src/Data.StandardDataSets/)

Common data sets like country lists and regions.

### [Data Utilities](src/Data.Utilities/)

Various utilities for working with data.
