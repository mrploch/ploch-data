# Ploch.Data.GenericRepository.EFCore.SqlServer

One-call dependency-injection registration of a SQL Server `DbContext` together with the full
generic repository and Unit of Work stack.

## Quick start

```csharp
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

builder.Services.AddDbContextWithRepositories<MyDbContext>();
```

That registers the `DbContext` against SQL Server, every repository interface, and `IUnitOfWork`.
To move to SQLite, swap this package for
[.SqLite](https://www.nuget.org/packages/Ploch.Data.GenericRepository.EFCore.SqLite/) and update `appsettings.json` —
no code changes needed.

## Documentation

- [Dependency injection guide](https://github.com/mrploch/ploch-data/blob/main/docs/dependency-injection.md)
- [Full documentation](https://data.github.ploch.dev/)
