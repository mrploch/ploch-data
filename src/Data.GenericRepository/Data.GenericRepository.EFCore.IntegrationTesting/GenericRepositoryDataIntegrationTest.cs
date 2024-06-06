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
    GenericRepositoryDataIntegrationTest<TDbContext> : DataIntegrationTest<TDbContext> // TODO: Rename to GenericRepositoryIntegrationTest and re-use Ploch.Data.EFCore.IntegrationTesting.DataIntegrationTest
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