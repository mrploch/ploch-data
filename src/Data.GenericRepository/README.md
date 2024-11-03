# Generic Repository and Unit of Work

## Overview

The generic [Repository](https://martinfowler.com/eaaCatalog/repository.html) and
[Unit of Work](https://martinfowler.com/eaaCatalog/unitOfWork.html) pattern implementation
for .NET.

## Motivation

The repository and unit of work patterns are a common way of abstracting the data access layer.
They are often used in conjunction with an ORM like the [Entity Framework](https://docs.microsoft.com/en-us/ef/core/).

While many consider EF Core a repository and unit of work, it lacks the ability to be easily mocked with a simple
interfaces.
Some may argue that you can test your code against an in-memory database, it is not always the best option.
It also ties you to a particular ORM, even if, in some cases, you might not want to use the ORM at all.

## Ploch.Data.GenericRepository Overview

The `Ploch.Data.GenericRepository` library provides a simple implementation of the repository and unit of work patterns.

It delivers various types of entity repositories depending on the particular use case:

- `IReadRepository<TEntity>` - provides read-only access to entities.
- `IReadRepositoryAsync<TEntity>` - provides asynchronous read-only access to entities.
- `IWriteRepository<TEntity>` - provides write access to entities.
- `IWriteRepositoryAsync<TEntity>` - provides asynchronous write access to entities.
- `IReadWriteRepository<TEntity>` - provides read and write access to entities.

It also provides `IUnitOfWork` interface and implementation which allows you to commit changes to the database.

## Ploch.Data.GenericRepository.EFCore Overview

The `Ploch.Data.GenericRepository.EFCore` library provides an implementation of the repositories and `IUnitOfWork` for
the Entity Framework Core. Currently, this is the only supported ORM.

## Installation

You can install the `Ploch.Data.GenericRepository` and `Ploch.Data.GenericRepository.EFCore` libraries via NuGet.

```powershell
nuget install Ploch.Data.GenericRepository.EFCore
```

## Usage

1) Register the repositories and unit of work in your DI container.

```csharp
var services = new ServiceCollection();
services.AddDbContext<MyDbContext>(options => options.UseSqlServer("connectionString"));
services.AddRepositories<MyDbContext>();

var serviceProvider = services.BuildServiceProvider();

// Now you can inject the repositories and unit of work into your services
public class MyService(IReadRepositoryAsync<MyEntity, int> readRepository)
{
    public async Task<IEnumerable<MyEntity>> GetEntities()
    {
        return await readRepository.GetPageAsync(1, 10);
    }
    
    public async Task<MyEntity> GetEntity(int id)
    {
        return await readRepository.GetByIdAsync(id);
    }
}

public class MySaveDataService(IUnitOfWorkk unitOfWork)
{
    public async Task SaveData(MyEntity entity, OtherEntity otherEntity)
    {
        await unitOfWork.Repository<MyEntity, int>().AddAsync(entity);
        await unitOfWork.Repository<OtherEntity, int>().UpdateAsync(otherEntity);
        
        await unitOfWork.CommitAsync();
    }
}
```

Custom repositories can also be created by inheriting from one of the repository classes:
Code below will register you custom repository mapping from the following interfaces:

- `ICustomBlogRepository` to `CustomBlogRepository`
- `IReadWriteRepositoryAsync<Blog, int>` to `CustomBlogRepository`
- `IReadRepositoryAsync<Blog, int>` to `CustomBlogRepository`
- `IWriteRepositoryAsync<Blog, int>` to `CustomBlogRepository`
  As well as the synchronous versions of the interfaces:
- `IQueryableRepository<Blog>`
- `IReadWriteRepository<Blog, int>`
- `IReadRepository<Blog, int>`
- `IWriteRepository<Blog, int>`

```csharp

serviceCollection.AddCustomReadWriteRepository<ICustomBlogRepository, CustomBlogRepository, Blog, int>((collection, repositoryInterface, repositoryImpl) =>
                                                                                                                   collection.AddScoped(repositoryInterface,
                                                                                                                                        repositoryImpl));

public interface ICustomBlogRepository : IReadWriteRepositoryAsync<Blog, int>
{
    Task<IList<Blog>> GetBlogWithPostsAsync(int blogId, CancellationToken cancellationToken);
}

public class CustomBlogRepository(DbContext dbContext) : ReadWriteRepositoryAsync<Blog, int>(dbContext), ICustomBlogRepository
{
    public async Task<IList<Blog>> GetBlogWithPostsAsync(int blogId, CancellationToken cancellationToken)
    {
        return await Entities.Where(b => b.BlogPosts.Any()).ToListAsync(cancellationToken);
    }
}

public class MyBlogService(IReadWriteRepositoryAsync<Blog, int> blogRepository, ICustomBlogRepository customBlogRepository)
{
    public async Task<IList<Blog>> GetBlogWithPosts(int blogId)
    {
        var blogRepository == customBlogRepository; // true
        return await customBlogRepository.GetBlogWithPostsAsync(blogId, CancellationToken.None);
    }
}
```

## Testing

The `Ploch.Data.GenericRepository` library is designed to be easily testable - including unit and integration tests.
For the purpose of integration testing, there is `Ploch.Data.GenericRepository.IntegrationTesting` package which
provides a base class for integration tests that use the SqLite in-memory database (by default, but any provider can be
used).

Take a look
at [Ploch.Data.GenericRepository.IntegrationTests](https://github.com/mrploch/ploch-data/tree/main/tests/Data.GenericRepository/Data.GenericRepository.EFCore.IntegrationTests)
project.