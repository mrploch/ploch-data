# Integration Testing Guide

Ploch.Data provides base classes that simplify writing integration tests against a real database engine. By default, tests use an in-memory SQLite database, ensuring fast execution with no external dependencies.

## Packages

| Package | Provides |
|---------|----------|
| `Ploch.Data.EFCore.IntegrationTesting` | `DataIntegrationTest<TDbContext>` -- basic EF Core test base |
| `Ploch.Data.GenericRepository.EFCore.IntegrationTesting` | `GenericRepositoryDataIntegrationTest<TDbContext>` -- adds repository and UoW helpers |

## DataIntegrationTest\<TDbContext\>

The base class for integration tests that need a configured DbContext backed by an in-memory SQLite database.

### What It Provides

- **DbContext** -- a fully configured EF Core context with schema created.
- **ServiceProvider** -- an `IServiceProvider` with the DbContext registered, plus any services added via `ConfigureServices`.
- **Automatic disposal** -- the DbContext and service provider are disposed when the test class is disposed.

### Usage

````csharp
using Ploch.Data.EFCore.IntegrationTesting;

public class MyDbContextTests : DataIntegrationTest<MyDbContext>
{
    [Fact]
    public async Task CanAddAndQueryEntities()
    {
        DbContext.Products.Add(new Product { Title = "Widget" });
        await DbContext.SaveChangesAsync();

        var products = await DbContext.Products.ToListAsync();
        products.Should().ContainSingle();
    }
}
````

### Customising Services

Override `ConfigureServices` to register additional services:

````csharp
public class MyTests : DataIntegrationTest<MyDbContext>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddSingleton<IMyService, MyService>();
    }

    [Fact]
    public void ServiceIsAvailable()
    {
        var service = ServiceProvider.GetRequiredService<IMyService>();
        service.Should().NotBeNull();
    }
}
````

### Using a Different Database Provider

Pass a custom `IDbContextConfigurator` to use a different database:

````csharp
public class SqlServerTests : DataIntegrationTest<MyDbContext>
{
    public SqlServerTests()
        : base(new SqlServerDbContextConfigurator("Server=...;Database=TestDb"))
    { }
}
````

## GenericRepositoryDataIntegrationTest\<TDbContext\>

Extends `DataIntegrationTest<TDbContext>` with helper methods for creating repositories and Unit of Work instances. This is the recommended base class for testing repository-based code.

### What It Provides (in addition to DataIntegrationTest)

| Method                                                                        | Returns                                   |
|-------------------------------------------------------------------------------|-------------------------------------------|
| `CreateUnitOfWork(bool useScopedProvider = true)`                             | `IUnitOfWork`                             |
| `CreateQueryableRepository<TEntity>(bool useScopedProvider = true)`           | `IQueryableRepository<TEntity>`           |
| `CreateReadRepositoryAsync<TEntity, TId>(bool useScopedProvider = true)`      | `IReadRepositoryAsync<TEntity, TId>`      |
| `CreateReadWriteRepositoryAsync<TEntity, TId>(bool useScopedProvider = true)` | `IReadWriteRepositoryAsync<TEntity, TId>` |
| `CreateReadRepository<TEntity, TId>(bool useScopedProvider = true)`           | `IReadRepository<TEntity, TId>`           |
| `CreateReadWriteRepository<TEntity, TId>(bool useScopedProvider = true)`      | `IReadWriteRepository<TEntity, TId>`      |

All helper methods use the test's default scope, so every service they return shares one `DbContext` and one change tracker. Pass `useScopedProvider: false` to resolve from a **fresh scope** instead -- each such call gets its own scope, so the returned unit of work or repository has an independent `DbContext`. Those scopes are tracked by `DataIntegrationTest<TDbContext>` and disposed with the test, so no cleanup is needed.

Use the default (`true`) when the test wants to observe the effects of a repository through the same change tracker. Use `false` when the test needs to prove that something reached the database rather than the tracker -- although `CreateRootDbContext()` is usually the better tool for that, because it returns a context built by `IDbContextFactory<TDbContext>` outside any scope.

For an independent scope from which several services must be resolved together, call `CreateScope()` directly:

````csharp
var scope = CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
````

The `AddRepositories<TDbContext>()` call is made automatically in `ConfigureServices`.

### Basic Repository Test

````csharp
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;

public class ArticleRepositoryTests
    : GenericRepositoryDataIntegrationTest<SampleAppDbContext>
{
    [Fact]
    public async Task AddAsync_should_persist_article_with_audit_properties()
    {
        var repository = CreateReadWriteRepositoryAsync<Article, int>();

        var article = new Article
        {
            Title = "Test Article",
            Description = "A test article",
            Contents = "Some content"
        };

        await repository.AddAsync(article);
        await DbContext.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(article.Id);

        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Article");
        saved.CreatedTime.Should().NotBeNull();
        saved.ModifiedTime.Should().NotBeNull();
    }
}
````

### Testing with Eager Loading

````csharp
[Fact]
public async Task AddAsync_should_persist_article_with_categories_and_tags()
{
    var articleRepo = CreateReadWriteRepositoryAsync<Article, int>();
    var categoryRepo = CreateReadWriteRepositoryAsync<ArticleCategory, int>();
    var tagRepo = CreateReadWriteRepositoryAsync<ArticleTag, int>();

    var category = new ArticleCategory { Name = "Test Category" };
    await categoryRepo.AddAsync(category);

    var tag = new ArticleTag { Name = "Test Tag", Description = "A test tag" };
    await tagRepo.AddAsync(tag);

    var article = new Article
    {
        Title = "Test Article",
        Categories = new List<ArticleCategory> { category },
        Tags = new List<ArticleTag> { tag }
    };

    await articleRepo.AddAsync(article);
    await DbContext.SaveChangesAsync();

    var saved = await articleRepo.GetByIdAsync(
        article.Id,
        onDbSet: q => q.Include(a => a.Categories).Include(a => a.Tags));

    saved.Should().NotBeNull();
    saved!.Categories.Should().HaveCount(1);
    saved.Tags.Should().HaveCount(1);
}
````

### Testing Unit of Work

````csharp
[Fact]
public async Task CommitAsync_should_persist_changes_across_repositories()
{
    var unitOfWork = CreateUnitOfWork();

    var authorRepo = unitOfWork.Repository<Author, int>();
    var articleRepo = unitOfWork.Repository<Article, int>();

    var author = new Author { Name = "Test Author" };
    await authorRepo.AddAsync(author);

    var article = new Article { Title = "Test Article", Author = author };
    await articleRepo.AddAsync(article);

    await unitOfWork.CommitAsync();

    var savedAuthor = await authorRepo.GetByIdAsync(author.Id);
    var savedArticle = await articleRepo.GetByIdAsync(article.Id);

    savedAuthor.Should().NotBeNull();
    savedArticle.Should().NotBeNull();
    savedArticle!.AuthorId.Should().Be(savedAuthor!.Id);
}
````

### Testing Pagination

````csharp
[Fact]
public async Task GetPageAsync_should_return_paginated_results()
{
    var repository = CreateReadWriteRepositoryAsync<Article, int>();

    for (var i = 0; i < 10; i++)
    {
        await repository.AddAsync(new Article { Title = $"Article {i + 1}" });
    }
    await DbContext.SaveChangesAsync();

    var page = await repository.GetPageAsync(1, 5);

    page.Should().HaveCount(5);
}
````

### Testing Filtering

````csharp
[Fact]
public async Task GetAllAsync_with_predicate_should_filter_results()
{
    var repository = CreateReadWriteRepositoryAsync<Article, int>();

    await repository.AddAsync(new Article { Title = "C# Tutorial" });
    await repository.AddAsync(new Article { Title = "Java Guide" });
    await repository.AddAsync(new Article { Title = "C# Advanced" });
    await DbContext.SaveChangesAsync();

    var csharpArticles = await repository.GetAllAsync(
        a => a.Title.Contains("C#"));

    csharpArticles.Should().HaveCount(2);
}
````

## SqLiteDbContextConfigurator

The `SqLiteDbContextConfigurator` handles the details of SQLite in-memory database setup:

- For in-memory databases (`DataSource=:memory:`), it creates a single shared `SqliteConnection` that is reused across all DbContext instances. This is necessary because each new SQLite connection to `:memory:` creates a separate empty database.
- The connection is opened immediately and kept alive for the lifetime of the test.
- Implements `IDisposable` to clean up the shared connection.

You rarely need to interact with this directly -- the `DataIntegrationTest` base class creates it automatically when no configurator is provided.

## DbContextServicesRegistrationHelper

This static helper class builds the service provider and prepares the DbContext for testing:

1. Registers the DbContext in the service collection.
2. Builds the service provider.
3. Opens the database connection.
4. Calls `EnsureCreated()` to apply the schema.
5. Returns a `TestDbContextHarness<TDbContext>` owning everything it created.

If any of those steps fails, everything already created is released before the exception propagates, so a failing `EnsureCreated()` does not leak a provider, a scope or a connection.

### TestDbContextHarness\<TDbContext\>

`BuildHarness<TDbContext>(...)` is the preferred entry point. The returned harness is a single `IDisposable`/`IAsyncDisposable` that owns the root service provider, the initial scope and the initial `DbContext`, and exposes them as `RootServiceProvider`, `ScopedServiceProvider` and `DbContext`:

````csharp
var services = new ServiceCollection();
services.AddRepositories<MyDbContext>(configuration);

await using var harness = DbContextServicesRegistrationHelper.BuildHarness<MyDbContext>(services);
var repository = harness.ScopedServiceProvider.GetRequiredService<IReadWriteRepositoryAsync<Blog, int>>();
````

`BuildHarness` registers only the `DbContext` and its factory. Repositories and `IUnitOfWork` come from `AddRepositories<TDbContext>(configuration)`, which `GenericRepositoryDataIntegrationTest.ConfigureServices` calls for you — resolve them from the harness only when the service collection you passed in already has them, as above.

#### Connection ownership

The harness owns the shared SQLite connection **only on the connection-string overload**, which is the overload that creates it. On the `IDbContextConfigurator` overload — the one `DataIntegrationTest<TDbContext>` uses — the connection belongs to the configurator, so the caller must dispose the configurator in addition to the harness:

````csharp
using var configurator = new SqLiteDbContextConfigurator(SqLiteConnectionOptions.InMemory);
using var harness = DbContextServicesRegistrationHelper.BuildHarness<MyDbContext>(services, configurator);
// Disposing `harness` releases the context, the scope and the root provider.
// Disposing `configurator` closes the shared in-memory connection.
````

`DataIntegrationTest<TDbContext>` already does both in its own `Dispose`, so tests deriving from it have nothing extra to remember.

Disposal is resilient in both directions: a failure releasing one resource does not stop the others from being released, and the failures are rethrown afterwards (aggregated if more than one). The root provider is released *before* the shared connection, so a singleton whose own disposal touches the database still sees an open connection.

The older `BuildDbContextAndServiceProvider<TDbContext>(...)` overloads still return the `(RootProvider, ScopedProvider, DbContext)` tuple and are implemented on top of `BuildHarness`. They hand back references but no ownership, so the caller must dispose the parts itself -- prefer `BuildHarness`.

## Testing Patterns

### Arrange-Act-Assert

Structure tests with clear Arrange, Act, and Assert sections:

````csharp
[Fact]
public async Task DeleteAsync_should_remove_entity()
{
    // Arrange
    var unitOfWork = CreateUnitOfWork();
    var tagRepo = unitOfWork.Repository<ArticleTag, int>();
    var tag = new ArticleTag { Name = "Temporary Tag" };
    await tagRepo.AddAsync(tag);
    await unitOfWork.CommitAsync();
    var tagId = tag.Id;

    // Act
    await tagRepo.DeleteAsync(tag);
    await unitOfWork.CommitAsync();

    // Assert
    var deleted = await tagRepo.GetByIdAsync(tagId);
    deleted.Should().BeNull();
}
````

### Testing Audit Timestamps

````csharp
[Fact]
public async Task CommitAsync_should_update_modified_time_on_update()
{
    var unitOfWork = CreateUnitOfWork();
    var repo = unitOfWork.Repository<Article, int>();

    var article = new Article { Title = "Original" };
    await repo.AddAsync(article);
    await unitOfWork.CommitAsync();
    var createdTime = article.CreatedTime;

    await Task.Delay(50);

    article.Title = "Updated";
    await repo.UpdateAsync(article);
    await unitOfWork.CommitAsync();

    article.ModifiedTime.Should().BeAfter(createdTime!.Value);
}
````

### Testing Hierarchical Entities

````csharp
[Fact]
public async Task Should_persist_hierarchical_categories()
{
    var categoryRepo = CreateReadWriteRepositoryAsync<ArticleCategory, int>();

    var parent = new ArticleCategory
    {
        Name = "Parent",
        Children = new List<ArticleCategory>
        {
            new() { Name = "Child", Children = new List<ArticleCategory>
            {
                new() { Name = "Grandchild" }
            }}
        }
    };

    await categoryRepo.AddAsync(parent);
    await DbContext.SaveChangesAsync();

    var saved = await categoryRepo.GetByIdAsync(
        parent.Id,
        onDbSet: q => q.Include(c => c.Children!));

    saved.Should().NotBeNull();
    saved!.Children.Should().HaveCount(1);
    saved.Children!.First().Name.Should().Be("Child");
}
````

## See Also

- [Getting Started](getting-started.md) -- quick-start examples
- [Generic Repository Guide](generic-repository.md) -- repository operations
- [Sample Application Tests](../samples/SampleApp/tests/) -- real integration tests
