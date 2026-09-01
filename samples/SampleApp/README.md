# Ploch.Data Sample Application

A complete working example demonstrating all major features of the Ploch.Data libraries.
It is configured to use either SQLite or SQL Server databases with SQLite being the default.

Migrations are kept in provider-specific projects.

## What It Demonstrates

- **Entity modelling** with `Ploch.Data.Model` interfaces (`IHasId`, `IHasTitle`, `IHasDescription`, `IHasContents`, `IHasAuditProperties`, `IHasCategories`, `IHasTags`)
- **Common base types** -- `Category<T>` for hierarchical categories, `Tag<TId>` for flat tags, `Property<TValue>` for key/value metadata
- **DbContext setup** with assembly-scanned entity configurations
- **Targeting multiple databases** with provider-specific migrations.
- **SQLite DateTimeOffset workaround** via `ApplySqLiteDateTimeOffsetPropertiesFix`
- **Automatic audit timestamps** via `SaveChanges` override on `IHasAuditTimeProperties` entities
- **DI registration** using `AddRepositories<TDbContext>()`
- **Repository operations** -- `GetByIdAsync`, `GetAllAsync`, `GetPageAsync`, `CountAsync` with filtering and eager loading
- **Unit of Work** -- atomic multi-entity transactions with `CommitAsync`
- **SQLite design-time factory** for EF Core migrations tooling
- **Integration tests** using `GenericRepositoryDataIntegrationTest<TDbContext>` base class
- **A real CLI** built with [`Ploch.CommandLine.Spectre`](https://github.com/mrploch/ploch-commandline) -- `AppBuilder` wires
  `Microsoft.Extensions.Hosting` (configuration + dependency injection) into a Spectre.Console.Cli command app, and every
  operation is its own command class with its own options

## Project Structure

```
samples/SampleApp/
  src/
    Model/                    # Entity POCOs
      Article.cs              # Full-featured entity with audit, categories, tags
      ArticleCategory.cs      # Hierarchical category (extends Category<T>)
      ArticleTag.cs           # Tag entity (extends Tag<TId>)
      ArticleProperty.cs      # Key/value property (extends Property<string>)
      Author.cs               # Entity with INamed, IHasDescription, IHasAuditProperties
    Data/                     # DbContext, configurations, DI registration
      SampleAppDbContext.cs
      Configurations/
      ServiceCollectionRegistrations.cs
    Data.SQLite/              # SQLite design-time factory
      SampleAppDbContextFactory.cs
    Data.SqlServer/           # SQL Server design-time factory
      SampleAppDbContextFactory.cs
    ConsoleApp/               # Console application host (Ploch.CommandLine.Spectre)
      Program.cs              # AppBuilder host + command registration
      Commands/               # One class per operation
        SampleAppCommand.cs   # Base class opening a DI scope per command
        DemoCommand.cs
        SeedCommand.cs
        ListArticlesCommand.cs
        ShowArticleCommand.cs
        SearchArticlesCommand.cs
        UpdateArticleCommand.cs
        ListAuthorsCommand.cs
      Services/
        SampleDataSeeder.cs   # Shared seeding logic used by 'seed' and 'demo'
  tests/
    IntegrationTests/         # Integration tests
      ArticleRepositoryTests.cs
      UnitOfWorkTests.cs
      SampleAppCommandsTests.cs  # End-to-end tests for every CLI command
```

## Running the Console App

The console app is a Spectre.Console CLI hosted by
[`Ploch.CommandLine.Spectre`](https://github.com/mrploch/ploch-commandline). Each operation is a separate command class
registered in `Program.cs` through `AppBuilder.Create(args).ConfigureCommandApp(...)`.

```bash
cd samples/SampleApp/src/ConsoleApp
dotnet run -- --help
```

The SQLite database file (`sampleapp.db`, see `appsettings.json`) is created in the working directory the first time a
command needs it, so no migration step is required to try the app out. To exercise the EF Core migrations tooling
instead, see [Working with migrations](#working-with-migrations) below.

### Commands

| Command | What it demonstrates |
|---|---|
| `demo` | The full guided walkthrough -- seeding, eager loading, filtering, updating, pagination, and direct repository injection |
| `seed` | Writing several entity types atomically through `IUnitOfWork.CommitAsync` |
| `list` | `GetPageAsync` and `CountAsync` |
| `show <ARTICLE-ID>` | `GetByIdAsync` with `onDbSet` eager loading (`Include` / navigation collections) |
| `search <TEXT>` | `GetAllAsync` with a filter applied through `onDbSet` |
| `update <ARTICLE-ID> --title <TITLE>` | `UpdateAsync` plus automatic `ModifiedTime` audit tracking |
| `authors` | A directly injected `IReadRepositoryAsync<Author, int>` |

```bash
dotnet run -- demo --author "Ada Lovelace" --filler 20
dotnet run -- seed --filler 50 --keep
dotnet run -- list --all --page-size 4
dotnet run -- show 1
dotnet run -- search "Entity Framework"
dotnet run -- update 1 --title "A better title"
dotnet run -- authors
```

Every command supports `--help`, for example `dotnet run -- list --help`.

`AppBuilder` also reads `DEV_RUNTIME`-prefixed environment variables. Set
`DEV_RUNTIME_CONSOLE_EXIT_PAUSE=true` to make the host wait for Enter before exiting, which is convenient when the app
is launched from a window that closes on exit. The pause is off unless that variable is set, so the commands are safe
to script; if you see `Press Enter to exit...`, the variable is set somewhere in your environment.

### Working with migrations

Migrations live in the provider-specific projects. To create and apply them for SQLite:

```bash
cd samples/SampleApp/src/Data.SQLite
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Running the Tests

```bash
cd samples/SampleApp
dotnet test
```

The integration tests use an in-memory SQLite database and verify repository operations, Unit of Work transactions, audit timestamps, hierarchical categories, and pagination.

## Switching Between SQLite and SQL Server

The SampleApp uses the provider-specific DI packages, which allow switching the database with **zero application code changes**. Only two things need to change: the package reference and the connection string.

### Using SQLite (default)

In the ConsoleApp `.csproj`, reference the SQLite package:

```xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.SqLite" />
```

In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=sampleapp.db;Cache=Shared"
  }
}
```

### Using SQL Server

Swap the package reference to SQL Server:

```xml
<PackageReference Include="Ploch.Data.GenericRepository.EFCore.SqlServer" />
```

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SampleApp;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

Add migrations to the SqlServer project (see the `appsettings.json` file for the connection string):

```bash
cd samples/SampleApp/src/Data.SqlServer
dotnet ef migrations add InitialCreate
```

Then, you need to create the database:

```bash
dotnet ef database update
```

### Why No Code Changes Are Needed

Both packages expose the same namespace (`Ploch.Data.GenericRepository.EFCore.DependencyInjection`) and method (`AddDbContextWithRepositories<TDbContext>()`). The `Program.cs` call remains identical:

```csharp
builder.Services.AddDbContextWithRepositories<SampleAppDbContext>();
```

Behind the scenes, the SQLite package registers `SqLiteDbContextCreationLifecycle` (which applies the `DateTimeOffset` value converter fix), while the SQL Server package registers `DefaultDbContextCreationLifecycle` (no-op). The `SampleAppDbContext` accepts `IDbContextCreationLifecycle` via constructor injection and calls it from `OnModelCreating`, keeping the DbContext itself provider-agnostic.

See the [Dependency Injection Guide](../../docs/dependency-injection.md) for full details on all registration approaches.

## Key Code Examples

### Entity with full Ploch.Data.Model interfaces

The `Article` entity demonstrates implementing multiple interfaces:

```csharp
public class Article : IHasId<int>, IHasTitle, IHasDescription, IHasContents,
                       IHasAuditProperties,
                       IHasCategories<ArticleCategory>, IHasTags<ArticleTag>
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Contents { get; set; }
    // ... audit properties, navigation properties
}
```

### DI registration

```csharp
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;

// One call registers DbContext + repositories + lifecycle plugin.
// Connection string loaded from appsettings.json automatically.
builder.Services.AddDbContextWithRepositories<SampleAppDbContext>();
```

### Repository usage with eager loading

```csharp
var article = await readArticleRepo.GetByIdAsync(
    articleId,
    onDbSet: q => q.Include(a => a.Author)
                   .Include(a => a.Categories)
                   .Include(a => a.Tags)
                   .Include(a => a.Properties));
```

### Integration test base class

```csharp
public class ArticleRepositoryTests
    : GenericRepositoryDataIntegrationTest<SampleAppDbContext>
{
    [Fact]
    public async Task AddAsync_should_persist_article_with_audit_properties()
    {
        var repository = CreateReadWriteRepositoryAsync<Article, int>();
        // ... test code
    }
}
```

## Documentation

- [Ploch.Data documentation](../../docs/README.md) -- detailed guides on each library component.
- [Ploch.CommandLine](https://github.com/mrploch/ploch-commandline) -- the `Ploch.CommandLine.Spectre` packages used to
  build this CLI (`Ploch.CommandLine.Spectre`, plus the `.Serilog` and `.FluentValidation` companions).
- [Spectre.Console.Cli](https://spectreconsole.net/cli/) -- the command, settings, and argument-parsing model that
  `Ploch.CommandLine.Spectre` builds on.
