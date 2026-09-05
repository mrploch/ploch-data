# Ploch.Data.GenericRepository.EFCore.SqLite

One-call dependency-injection registration of a SQLite `DbContext` together with the full generic
repository and Unit of Work stack.

## Quick start

```csharp
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

builder.Services.AddDbContextWithRepositories<MyDbContext>();
```

That registers the `DbContext` against SQLite, every repository interface, and `IUnitOfWork`.
To move to SQL Server, swap this package for
[.SqlServer](https://github.com/mrploch/ploch-data/tree/main/src/Data.GenericRepository/Data.GenericRepository.EFCore.SqlServer/) and update `appsettings.json` —
no code changes needed.

## Documentation

- [Dependency injection guide](https://github.com/mrploch/ploch-data/blob/main/docs/dependency-injection.md)
- [Full documentation](https://data.github.ploch.dev/)
