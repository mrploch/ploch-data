# Ploch.Data.EFCore.SqLite

SQLite provider package for Ploch.Data -- includes a design-time factory, runtime configurator, and a workaround for SQLite's lack of `DateTimeOffset` support.

## Key Features

- **SqLiteDbContextFactory\<TDbContext, TFactory\>** -- one-line design-time factory for SQLite
- **SqLiteDbContextConfigurator** -- runtime configurator with shared in-memory connection support (critical for testing)
- **SqLiteDateTimeOffsetPropertiesFix** -- model builder extension that applies `DateTimeOffsetToBinaryConverter` to all `DateTimeOffset` properties when SQLite is detected
- **SqLiteConnectionOptions** -- connection string builder with `InMemory` preset

## Installation

```xml
<PackageReference Include="Ploch.Data.EFCore.SqLite" />
```

## Quick Start

### Design-time factory

```csharp
using Ploch.Data.EFCore.SqLite;

public class MyDbContextFactory()
    : SqLiteDbContextFactory<MyDbContext, MyDbContextFactory>(
        options => new MyDbContext(options));
```

### DateTimeOffset fix

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplySqLiteDateTimeOffsetPropertiesFix(Database);
    base.OnModelCreating(modelBuilder);
}
```

### Runtime configuration (integration testing)

```csharp
var configurator = new SqLiteDbContextConfigurator(SqLiteConnectionOptions.InMemory);
services.AddDbContext<MyDbContext>(configurator.Configure);
```

## Documentation

See the [Data Project Setup Guide](../../docs/data-project-setup.md) for full provider project setup and the [Integration Testing Guide](../../docs/integration-testing.md) for testing patterns.
