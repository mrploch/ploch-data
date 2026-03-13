# Ploch.Data.GenericRepository.EFCore.DependencyInjection

Integration with the Ploch.Common `ServicesBundle` pattern for registering Generic Repository services.

## Key Features

- **GenericRepositoriesServicesBundle\<TDbContext\>** -- abstract base class for creating a `ServicesBundle` that registers DbContext and all repository interfaces

## Installation

```xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.DependencyInjection" />
```

## Quick Start

```csharp
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

public class MyDataBundle : GenericRepositoriesServicesBundle<MyDbContext>
{
    protected override Action<DbContextOptionsBuilder> GetOptionsBuilderAction(
        IConfiguration? configuration)
    {
        return options => options.UseSqlite(
            configuration!.GetConnectionString("DefaultConnection"));
    }
}

// Register in application startup
services.AddServicesBundle(new MyDataBundle(), configuration);
```

## Documentation

See the [Generic Repository Guide](../../../docs/generic-repository.md) for DI registration patterns.
