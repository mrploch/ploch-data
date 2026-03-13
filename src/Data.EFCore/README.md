# Ploch.Data.EFCore

Core EF Core utilities for the Ploch.Data libraries -- design-time DbContext factory base classes, configurators, data seeding, and value converters.

## Key Features

- **BaseDbContextFactory\<TDbContext, TFactory\>** -- abstract base for design-time factories with automatic migrations assembly configuration
- **IDbContextConfigurator** -- abstraction for runtime DbContext configuration (used in DI and integration tests)
- **ConnectionString** -- helper to read connection strings from `appsettings.json`
- **DataSeeder** -- utilities for seeding data into the database
- **CollectionStringSplitConverter** -- EF Core value converter for storing string collections as delimited strings

## Installation

```xml
<PackageReference Include="Ploch.Data.EFCore" />
```

For most applications, use one of the provider-specific packages instead:
- `Ploch.Data.EFCore.SqLite`
- `Ploch.Data.EFCore.SqlServer`

## Quick Start

```csharp
using Ploch.Data.EFCore;

// Create a design-time factory by inheriting from a provider-specific base
public class MyDbContextFactory : BaseDbContextFactory<MyDbContext, MyDbContextFactory>
{
    public MyDbContextFactory() : base(options => new MyDbContext(options)) { }

    protected override DbContextOptionsBuilder<MyDbContext> ConfigureOptions(
        Func<string> connectionStringFunc,
        DbContextOptionsBuilder<MyDbContext> optionsBuilder)
    {
        return optionsBuilder.UseSqlite(
            connectionStringFunc(), ApplyMigrationsAssembly);
    }
}
```

## Documentation

See the [Data Project Setup Guide](../../docs/data-project-setup.md) for step-by-step instructions and the [Architecture Overview](../../docs/architecture.md) for package details.
