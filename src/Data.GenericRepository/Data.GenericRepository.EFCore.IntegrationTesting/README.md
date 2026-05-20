# Ploch.Data.GenericRepository.EFCore.IntegrationTesting

Base class for integration tests using the Generic Repository and Unit of Work with an in-memory SQLite database.

## Key Features

- **GenericRepositoryDataIntegrationTest\<TDbContext\>** -- pre-configured test base with repository and UoW helpers
- **In-memory SQLite** -- fast, isolated tests with no external database required
- **Repository helpers** -- `CreateQueryableRepository`, `CreateReadRepositoryAsync`, `CreateReadRepository`, `CreateReadWriteRepositoryAsync`, `CreateReadWriteRepository`, and `CreateUnitOfWork` (each supports optional `bool useScopedProvider = true`)
- **Automatic DI** -- repositories and Unit of Work registered automatically via `AddRepositories<TDbContext>()`

## Installation

```xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.IntegrationTesting" />
```

## Quick Start

```csharp
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;

public class ProductTests : GenericRepositoryDataIntegrationTest<MyDbContext>
{
    [Fact]
    public async Task AddAsync_should_persist_product()
    {
        var repo = CreateReadWriteRepositoryAsync<Product, int>();

        await repo.AddAsync(new Product { Title = "Widget" });
        await DbContext.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        all.Should().ContainSingle();
    }

    [Fact]
    public async Task UnitOfWork_should_commit_atomically()
    {
        var uow = CreateUnitOfWork();
        var repo = uow.Repository<Product, int>();

        await repo.AddAsync(new Product { Title = "Test" });
        await uow.CommitAsync();

        var saved = await repo.GetAllAsync();
        saved.Should().ContainSingle();
    }
}
```

## Documentation

See the [Integration Testing Guide](../../../docs/integration-testing.md) for detailed patterns and examples.
