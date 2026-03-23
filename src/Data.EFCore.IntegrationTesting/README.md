# Ploch.Data.EFCore.IntegrationTesting

Base class and helpers for EF Core integration tests using in-memory SQLite databases.

## Key Features

- **DataIntegrationTest\<TDbContext\>** -- abstract base class that configures an in-memory SQLite database, creates the schema, and provides `DbContext` and `ServiceProvider` properties
- **DbContextServicesRegistrationHelper** -- static helper for building the service provider and preparing the DbContext
- **Custom configurator support** -- pass any `IDbContextConfigurator` to use a different database provider

## Installation

```xml
<PackageReference Include="Ploch.Data.EFCore.IntegrationTesting" />
```

For repository and Unit of Work testing helpers, use `Ploch.Data.GenericRepository.EFCore.IntegrationTesting` instead.

## Quick Start

```csharp
using Ploch.Data.EFCore.IntegrationTesting;

public class MyDbContextTests : DataIntegrationTest<MyDbContext>
{
    [Fact]
    public async Task CanAddEntities()
    {
        DbContext.Products.Add(new Product { Title = "Test" });
        await DbContext.SaveChangesAsync();

        var count = await DbContext.Products.CountAsync();
        count.Should().Be(1);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        // Register additional services here
    }
}
```

## Documentation

See the [Integration Testing Guide](../../docs/integration-testing.md) for detailed patterns, examples, and testing strategies.
