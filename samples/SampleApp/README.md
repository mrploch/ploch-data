# Ploch.Data Sample Application

A complete working example demonstrating all major features of the Ploch.Data libraries.

## What It Demonstrates

- **Entity modelling** with `Ploch.Data.Model` interfaces (`IHasId`, `IHasTitle`, `IHasDescription`, `IHasContents`, `IHasAuditProperties`, `IHasCategories`, `IHasTags`)
- **Common base types** -- `Category<T>` for hierarchical categories, `Tag<TId>` for flat tags, `Property<TValue>` for key/value metadata
- **DbContext setup** with assembly-scanned entity configurations
- **SQLite DateTimeOffset workaround** via `ApplySqLiteDateTimeOffsetPropertiesFix`
- **Automatic audit timestamps** via `SaveChanges` override on `IHasAuditTimeProperties` entities
- **DI registration** using `AddRepositories<TDbContext>()`
- **Repository operations** -- `GetByIdAsync`, `GetAllAsync`, `GetPageAsync`, `CountAsync` with filtering and eager loading
- **Unit of Work** -- atomic multi-entity transactions with `CommitAsync`
- **SQLite design-time factory** for EF Core migrations tooling
- **Integration tests** using `GenericRepositoryDataIntegrationTest<TDbContext>` base class

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
    ConsoleApp/               # Console application host
      Program.cs
  tests/
    IntegrationTests/         # Integration tests
      ArticleRepositoryTests.cs
      UnitOfWorkTests.cs
```

## Running the Console App

```bash
cd samples/SampleApp/src/ConsoleApp
dotnet run
```

The console app creates sample entities (authors, categories, tags, articles), demonstrates CRUD operations, pagination, filtering, and eager loading.

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

See the [full documentation](../../docs/README.md) for detailed guides on each library component.
