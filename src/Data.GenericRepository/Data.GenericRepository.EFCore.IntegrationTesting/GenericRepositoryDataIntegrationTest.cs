using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.Data.Model;
using Ploch.Data.EFCore.IntegrationTesting;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

/// <summary>
///     Base class for integration tests that use EF Core in-memory SQLite database.
/// </summary>
/// <typeparam name="TDbContext">The data context type.</typeparam>
public abstract class
    GenericRepositoryDataIntegrationTest<TDbContext> : DataIntegrationTest<TDbContext>
    where TDbContext : DbContext
{
    protected GenericRepositoryDataIntegrationTest(IDbContextConfigurator? dbContextConfigurator = null) : base(dbContextConfigurator)
    { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddRepositories<TDbContext>();
    }

    protected IUnitOfWork CreateUnitOfWork()
    {
        return ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "The type name created ends with Async hence the name.")]
    protected IReadRepositoryAsync<TEntity, TId> CreateReadRepositoryAsync<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadRepositoryAsync<TEntity, TId>>();
    }

    protected IReadRepository<TEntity, TId> CreateReadRepository<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadRepository<TEntity, TId>>();
    }

    protected IReadWriteRepository<TEntity, TId> CreateReadWriteRepository<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadWriteRepository<TEntity, TId>>();
    }

    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "The type name created ends with Async hence the name.")]
    protected IReadWriteRepositoryAsync<TEntity, TId> CreateReadWriteRepositoryAsync<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadWriteRepositoryAsync<TEntity, TId>>();
    }

    protected IReadWriteRepository<TEntity, TId> CreateRepository<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadWriteRepository<TEntity, TId>>();
    }
}