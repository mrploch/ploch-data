# Ploch.Data.GenericRepository.EFCore.DependencyInjection

Integration with the Ploch.Common `ServicesBundle` pattern for registering Generic Repository services.

## Key Features

- **GenericRepositoriesServicesBundle\<TDbContext\>** -- abstract base class for creating a `ServicesBundle` that registers DbContext and all repository interfaces

## Installation

```xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.DependencyInjection" />
```

## Quick Start

> **Note:** The example below uses SQLite (`UseSqlite`). Replace with your chosen EF Core provider
> (e.g. `UseSqlServer`, `UseNpgsql`) and its corresponding NuGet package.

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

See the [Generic Repository Guide](https://github.com/mrploch/ploch-data/blob/main/docs/generic-repository.md) for DI registration patterns.
