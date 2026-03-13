# Ploch.Data.EFCore.SqlServer

SQL Server provider package for Ploch.Data -- includes a design-time factory and runtime configurator.

## Key Features

- **SqlServerDbContextFactory\<TDbContext, TFactory\>** -- one-line design-time factory for SQL Server
- **SqlServerDbContextConfigurator** -- runtime configurator for DI and testing

## Installation

```xml
<PackageReference Include="Ploch.Data.EFCore.SqlServer" />
```

## Quick Start

### Design-time factory

```csharp
using Ploch.Data.EFCore.SqlServer;

public class MyDbContextFactory()
    : SqlServerDbContextFactory<MyDbContext, MyDbContextFactory>(
        options => new MyDbContext(options));
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

## Documentation

See the [Data Project Setup Guide](../../docs/data-project-setup.md) for full provider project setup.
