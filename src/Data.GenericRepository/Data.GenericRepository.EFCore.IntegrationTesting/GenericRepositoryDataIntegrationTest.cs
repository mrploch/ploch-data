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
public abstract class GenericRepositoryDataIntegrationTest<TDbContext>(IDbContextConfigurator? dbContextConfigurator = null)
    : DataIntegrationTest<TDbContext>(dbContextConfigurator)
    where TDbContext : DbContext
{
    /// <summary>
    /// Configures the required services for the test.
    /// </summary>
    /// <param name="services">The service collection.</param>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddRepositories<TDbContext>();
    }

    /// <summary>
    ///    Creates a new unit of work.
    /// </summary>
    /// <returns>The unit of work.</returns>
    protected IUnitOfWork CreateUnitOfWork()
    {
        return ServiceProvider.GetRequiredService<IUnitOfWork>();
    }

    /// <summary>
    ///   Creates an instance of <see cref="IReadRepositoryAsync{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>An instance of <see cref="IReadWriteRepositoryAsync{TEntity,TId}"/></returns>
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "The type name created ends with Async hence the name.")]
    protected IReadRepositoryAsync<TEntity, TId> CreateReadRepositoryAsync<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadRepositoryAsync<TEntity, TId>>();
    }

    /// <summary>
    ///  Creates a <see cref="IReadRepository{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>An instance of <see cref="IReadWriteRepository{TEntity,TId}"/>.</returns>
    protected IReadRepository<TEntity, TId> CreateReadRepository<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadRepository<TEntity, TId>>();
    }

    /// <summary>
    /// Creates a <see cref="IReadWriteRepository{TEntity,TId}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>An instance of <see cref="IReadWriteRepository{TEntity,TId}"/>.</returns>
    protected IReadWriteRepository<TEntity, TId> CreateReadWriteRepository<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadWriteRepository<TEntity, TId>>();
    }

    /// <summary>
    /// Creates a <see cref="IReadWriteRepositoryAsync{TEntity,TId}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>An instance of <see cref="IReadWriteRepositoryAsync{TEntity,TId}"/>.</returns>
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "The type name created ends with Async hence the name.")]
    protected IReadWriteRepositoryAsync<TEntity, TId> CreateReadWriteRepositoryAsync<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        return ServiceProvider.GetRequiredService<IReadWriteRepositoryAsync<TEntity, TId>>();
    }
}