# Ploch.Data.GenericRepository.EFCore.Specification

[Ardalis.Specification](https://github.com/ardalis/Specification) integration for the Ploch.Data Generic Repository, enabling composable, reusable query logic.

## Key Features

- **GetAllBySpecificationAsync** -- query multiple entities using a `Specification<TEntity>`
- **GetBySpecificationAsync** -- query a single entity using a `SingleResultSpecification<TEntity>`
- Extension methods on `IQueryableRepository<TEntity>` -- works with any repository

## Installation

```xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.Specification" />
```

## Quick Start

```csharp
using Ardalis.Specification;
using Ploch.Data.GenericRepository.EFCore.Specification;

// Define a specification
public class ActiveProductsSpec : Specification<Product>
{
    public ActiveProductsSpec(string? nameContains = null)
    {
        Query
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .Where(p => p.Name.Contains(nameContains!), nameContains is not null);
    }
}

// Use it
var activeProducts = await repository.GetAllBySpecificationAsync(
    new ActiveProductsSpec("Widget"));
```

## Documentation

See the [Generic Repository Guide](../../../docs/generic-repository.md#specification-pattern-ardalisspecification) for detailed specification patterns.
