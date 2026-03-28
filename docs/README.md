# Ploch.Data Documentation

Comprehensive documentation for the Ploch.Data libraries -- a set of .NET packages for building data access layers using standardised entity models, the Generic Repository and Unit of Work patterns, and Entity Framework Core.

## Documentation Index

| Document | Description |
|----------|-------------|
| [Getting Started](getting-started.md) | Quick start guides for common use cases |
| [Data Model](data-model.md) | Complete guide to `Ploch.Data.Model` interfaces and common types |
| [Generic Repository](generic-repository.md) | Repository and Unit of Work patterns, DI registration, Specification support |
| [Data Project Setup](data-project-setup.md) | Step-by-step guide for creating Data and provider projects |
| [Integration Testing](integration-testing.md) | Testing with in-memory SQLite, base test classes, and patterns |
| [Extending the Libraries](extending.md) | Custom repositories, new providers, and extensibility points |
| [Architecture Overview](architecture.md) | Package dependencies, high-level design, and layer diagrams |

## Package Overview

| Package | Purpose |
|---------|---------|
| `Ploch.Data.Model` | Standardised entity interfaces (`IHasId`, `INamed`, `IHasAuditProperties`, etc.) and common base types (`Category`, `Tag`, `Property`) |
| `Ploch.Data.EFCore` | EF Core utilities: design-time factory base classes, `IDbContextConfigurator`, data seeding helpers |
| `Ploch.Data.EFCore.SqLite` | SQLite provider: `SqLiteDbContextFactory`, `SqLiteDbContextConfigurator`, `DateTimeOffset` workaround |
| `Ploch.Data.EFCore.SqlServer` | SQL Server provider: `SqlServerDbContextFactory`, `SqlServerDbContextConfigurator` |
| `Ploch.Data.GenericRepository` | Provider-agnostic repository and Unit of Work interfaces (`IReadRepositoryAsync`, `IReadWriteRepositoryAsync`, `IUnitOfWork`) |
| `Ploch.Data.GenericRepository.EFCore` | EF Core implementations of the Generic Repository and Unit of Work, plus DI registration |
| `Ploch.Data.GenericRepository.EFCore.DependencyInjection` | `GenericRepositoriesServicesBundle` for the Ploch.Common `ServicesBundle` pattern |
| `Ploch.Data.GenericRepository.EFCore.Specification` | Ardalis.Specification integration for composable, reusable query logic |
| `Ploch.Data.EFCore.IntegrationTesting` | `DataIntegrationTest<TDbContext>` base class for EF Core integration tests |
| `Ploch.Data.GenericRepository.EFCore.IntegrationTesting` | `GenericRepositoryDataIntegrationTest<TDbContext>` base class with repository/UoW helpers |

## Sample Application

A fully working [Sample Application](../samples/SampleApp/) demonstrates all major features including entity modelling, repository usage, Unit of Work, pagination, eager loading, and integration testing.

## Additional Resources

- [API Documentation](https://github.ploch.dev/ploch-data/) (GitHub Pages)
- [Repository](https://github.com/mrploch/ploch-data)
