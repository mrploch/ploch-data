# Extending the Libraries

Ploch.Data is designed to be extensible at multiple levels: custom repositories, new database providers, alternative ORMs, and custom entity interfaces. This guide covers each extensibility point.

## Creating Custom Repositories

When the generic repository does not cover a specific query or business operation, create a custom repository that inherits from the EF Core implementation.

### Step 1: Define the interface

````csharp
using Ploch.Data.GenericRepository;

public interface IArticleRepository : IReadWriteRepositoryAsync<Article, int>
{
    Task<IList<Article>> GetPublishedWithAuthorsAsync(
        CancellationToken ct = default);

    Task<Article?> GetBySlugAsync(string slug,
        CancellationToken ct = default);
}
````

### Step 2: Implement the repository

````csharp
using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository;
using Ploch.Data.GenericRepository.EFCore;

public class ArticleRepository(DbContext dbContext, IAuditEntityHandler auditHandler)
    : ReadWriteRepositoryAsync<Article, int>(dbContext, auditHandler),
      IArticleRepository
{
    public async Task<IList<Article>> GetPublishedWithAuthorsAsync(
        CancellationToken ct = default)
    {
        return await Entities
            .Where(a => a.IsPublished)
            .Include(a => a.Author)
            .OrderByDescending(a => a.CreatedTime)
            .ToListAsync(ct);
    }

    public async Task<Article?> GetBySlugAsync(string slug,
        CancellationToken ct = default)
    {
        return await Entities
            .Include(a => a.Author)
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Slug == slug, ct);
    }
}
````

### Step 3: Register the custom repository

````csharp
using Ploch.Data.GenericRepository.EFCore;

services.AddCustomReadWriteAsyncRepository<
    IArticleRepository, ArticleRepository, Article, int>();
````

This registers the custom type for all matching interface types:
- `IArticleRepository`
- `IReadWriteRepositoryAsync<Article, int>`
- `IWriteRepositoryAsync<Article, int>`
- `IReadRepositoryAsync<Article, int>`
- `IReadRepositoryAsync<Article>`
- `IQueryableRepository<Article>`

You can then inject either the custom interface or any of the generic interfaces -- they all resolve to the same `ArticleRepository` instance.

### Overriding Base Behaviour

You can override virtual methods on the base repository to customise default CRUD operations:

````csharp
public class ArticleRepository(DbContext dbContext, IAuditEntityHandler auditHandler)
    : ReadWriteRepositoryAsync<Article, int>(dbContext, auditHandler),
      IArticleRepository
{
    public override async Task UpdateAsync(Article entity,
        CancellationToken ct = default)
    {
        // Custom pre-update logic
        entity.Slug = GenerateSlug(entity.Title);

        await base.UpdateAsync(entity, ct);
    }
}
````

## Adding Support for Different Database Providers

To add a new EF Core provider (e.g., PostgreSQL), create a new factory class inheriting from `BaseDbContextFactory`.

### Step 1: Create the factory

````csharp
using Microsoft.EntityFrameworkCore;
using Ploch.Data.EFCore;

public class PostgresDbContextFactory<TDbContext, TFactory>
    : BaseDbContextFactory<TDbContext, TFactory>
    where TDbContext : DbContext
    where TFactory : BaseDbContextFactory<TDbContext, TFactory>
{
    protected PostgresDbContextFactory(
        Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator)
        : base(dbContextCreator) { }

    protected override DbContextOptionsBuilder<TDbContext> ConfigureOptions(
        Func<string> connectionStringFunc,
        DbContextOptionsBuilder<TDbContext> optionsBuilder)
    {
        return optionsBuilder.UseNpgsql(
            connectionStringFunc(), ApplyMigrationsAssembly);
    }
}
````

### Step 2: Create a concrete factory for your DbContext

````csharp
public class MyAppDbContextFactory()
    : PostgresDbContextFactory<MyAppDbContext, MyAppDbContextFactory>(
        options => new MyAppDbContext(options));
````

### Step 3: Create an IDbContextConfigurator (for DI and testing)

````csharp
public class PostgresDbContextConfigurator(string connectionString)
    : IDbContextConfigurator
{
    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(connectionString);
    }
}
````

## Extending to Non-EF Core ORMs

The `Ploch.Data.GenericRepository` package defines provider-agnostic interfaces. The EF Core implementations in `Ploch.Data.GenericRepository.EFCore` are just one possible set of implementations.

### Implementing with Dapper

To create a Dapper-backed implementation, implement the repository interfaces directly:

````csharp
using System.Data;
using Dapper;
using Ploch.Data.GenericRepository;
using Ploch.Data.Model;

public class DapperReadRepository<TEntity, TId>
    : IReadRepositoryAsync<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    private readonly IDbConnection _connection;
    private readonly string _tableName;

    public DapperReadRepository(IDbConnection connection)
    {
        _connection = connection;
        _tableName = typeof(TEntity).Name + "s"; // Simple pluralisation
    }

    public IQueryable<TEntity> Entities =>
        throw new NotSupportedException(
            "IQueryable is not supported by Dapper");

    public async Task<TEntity?> GetByIdAsync(TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
        CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";
        return await _connection.QueryFirstOrDefaultAsync<TEntity>(
            sql, new { Id = id });
    }

    public async Task<IList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? query = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
        CancellationToken cancellationToken = default)
    {
        var sql = $"SELECT * FROM {_tableName}";
        var result = await _connection.QueryAsync<TEntity>(sql);
        return result.ToList();
    }

    // ... implement remaining methods
}
````

### Register the Dapper implementation

````csharp
services.AddScoped<IDbConnection>(
    _ => new SqlConnection(connectionString));
services.AddScoped(
    typeof(IReadRepositoryAsync<,>),
    typeof(DapperReadRepository<,>));
````

## Extending to Non-SQL Databases

The same principle applies to NoSQL databases. Implement the repository interfaces using your chosen client library.

### MongoDB Example (Outline)

````csharp
using MongoDB.Driver;
using Ploch.Data.GenericRepository;
using Ploch.Data.Model;

public class MongoReadWriteRepository<TEntity, TId>
    : IReadWriteRepositoryAsync<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    private readonly IMongoCollection<TEntity> _collection;

    public MongoReadWriteRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<TEntity>(
            typeof(TEntity).Name);
    }

    public async Task<TEntity?> GetByIdAsync(TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<TEntity>.Filter.Eq("Id", id);
        return await _collection
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(entity, null, cancellationToken);
        return entity;
    }

    // ... implement remaining methods
}
````

**Note:** The `IQueryable<TEntity> Entities` property and `onDbSet` parameter are EF Core concepts. For non-EF implementations, you may throw `NotSupportedException` for `Entities` and ignore the `onDbSet` parameter, or provide a MongoDB LINQ-based `IQueryable` if the driver supports it.

## Creating Custom Entity Interfaces

You can create your own entity interfaces that follow the same pattern as `Ploch.Data.Model`:

````csharp
// Define the interface
public interface IHasPriority
{
    int Priority { get; set; }
}

public interface IHasSlug
{
    string Slug { get; set; }
}

// Use in entities
public class Article : IHasId<int>, IHasTitle, IHasPriority, IHasSlug
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int Priority { get; set; }
    public string Slug { get; set; } = null!;
}
````

Custom interfaces work seamlessly with the Generic Repository since repositories only require `IHasId<TId>`.

## Creating a Custom IAuditEntityHandler

To customise audit behaviour (e.g., use a different timestamp source or populate additional fields):

````csharp
public class CustomAuditHandler : IAuditEntityHandler
{
    private readonly ITimeProvider _timeProvider;
    private readonly ICurrentUserService _userService;

    public CustomAuditHandler(
        ITimeProvider timeProvider,
        ICurrentUserService userService)
    {
        _timeProvider = timeProvider;
        _userService = userService;
    }

    public void HandleCreation(object entity)
    {
        if (entity is IHasCreatedTime created)
            created.CreatedTime = _timeProvider.UtcNow;
        if (entity is IHasCreatedBy createdBy)
            createdBy.CreatedBy = _userService.GetCurrentUserId();
    }

    public void HandleModification(object entity)
    {
        if (entity is IHasModifiedTime modified)
            modified.ModifiedTime = _timeProvider.UtcNow;
        if (entity is IHasModifiedBy modifiedBy)
            modifiedBy.LastModifiedBy = _userService.GetCurrentUserId();
    }

    public bool HandleAccess(object entity) => false;
}

// Register after AddRepositories to override the default
services.AddSingleton<IAuditEntityHandler, CustomAuditHandler>();
````

## See Also

- [Generic Repository Guide](generic-repository.md) -- base repository operations and DI
- [Architecture Overview](architecture.md) -- package dependencies and design
