using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTesting;

/// <summary>
///     Base class for integration tests that use EF Core in-memory SQLite database.
/// </summary>
/// <typeparam name="TDbContext">The data context type.</typeparam>
public abstract class GenericRepositoryDataIntegrationTest<TDbContext>(IDbContextConfigurator? dbContextConfigurator = null)
    : DataIntegrationTest<TDbContext>(dbContextConfigurator) where TDbContext : DbContext
{
    /// <summary>
    ///     Configures the required services for the test.
    /// </summary>
    /// <param name="services">The service collection.</param>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.AddInMemoryCollection().Build();
        services.AddRepositories<TDbContext>(configuration);
    }

    /// <summary>
    ///     Creates a new unit of work.
    /// </summary>
    /// <param name="useScopedProvider">
    ///     <see langword="true" /> to resolve from <see cref="DataIntegrationTest{TDbContext}.ScopedServiceProvider" />;
    ///     otherwise resolve from <see cref="DataIntegrationTest{TDbContext}.RootServiceProvider" />.
    /// </param>
    /// <returns>The unit of work.</returns>
    protected IUnitOfWork CreateUnitOfWork(bool useScopedProvider = true) => GetServiceProvider(useScopedProvider).GetRequiredService<IUnitOfWork>();

    /// <summary>
    ///     Creates an instance of <see cref="IQueryableRepository{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="useScopedProvider">
    ///     <see langword="true" /> to resolve from <see cref="DataIntegrationTest{TDbContext}.ScopedServiceProvider" />;
    ///     otherwise resolve from <see cref="DataIntegrationTest{TDbContext}.RootServiceProvider" />.
    /// </param>
    /// <returns>An instance of <see cref="IQueryableRepository{TEntity}" />.</returns>
    protected IQueryableRepository<TEntity> CreateQueryableRepository<TEntity>(bool useScopedProvider = true) where TEntity : class =>
        GetServiceProvider(useScopedProvider).GetRequiredService<IQueryableRepository<TEntity>>();

    /// <summary>
    ///     Creates an instance of <see cref="IReadRepositoryAsync{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="useScopedProvider">
    ///     <see langword="true" /> to resolve from <see cref="DataIntegrationTest{TDbContext}.ScopedServiceProvider" />;
    ///     otherwise resolve from <see cref="DataIntegrationTest{TDbContext}.RootServiceProvider" />.
    /// </param>
    /// <returns>An instance of a <see cref="IReadWriteRepositoryAsync{TEntity,TId}" />.</returns>
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "The type name created ends with Async hence the name.")]
    protected IReadRepositoryAsync<TEntity, TId> CreateReadRepositoryAsync<TEntity, TId>(bool useScopedProvider = true) where TEntity : class, IHasId<TId> =>
        GetServiceProvider(useScopedProvider).GetRequiredService<IReadRepositoryAsync<TEntity, TId>>();

    /// <summary>
    ///     Creates a <see cref="IReadRepository{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="useScopedProvider">
    ///     <see langword="true" /> to resolve from <see cref="DataIntegrationTest{TDbContext}.ScopedServiceProvider" />;
    ///     otherwise resolve from <see cref="DataIntegrationTest{TDbContext}.RootServiceProvider" />.
    /// </param>
    /// <returns>An instance of <see cref="IReadWriteRepository{TEntity,TId}" />.</returns>
    protected IReadRepository<TEntity, TId> CreateReadRepository<TEntity, TId>(bool useScopedProvider = true) where TEntity : class, IHasId<TId> =>
        GetServiceProvider(useScopedProvider).GetRequiredService<IReadRepository<TEntity, TId>>();

    /// <summary>
    ///     Creates a <see cref="IReadWriteRepository{TEntity,TId}" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="useScopedProvider">
    ///     <see langword="true" /> to resolve from <see cref="DataIntegrationTest{TDbContext}.ScopedServiceProvider" />;
    ///     otherwise resolve from <see cref="DataIntegrationTest{TDbContext}.RootServiceProvider" />.
    /// </param>
    /// <returns>An instance of <see cref="IReadWriteRepository{TEntity,TId}" />.</returns>
    protected IReadWriteRepository<TEntity, TId> CreateReadWriteRepository<TEntity, TId>(bool useScopedProvider = true) where TEntity : class, IHasId<TId> =>
        GetServiceProvider(useScopedProvider).GetRequiredService<IReadWriteRepository<TEntity, TId>>();

    /// <summary>
    ///     Creates a <see cref="IReadWriteRepositoryAsync{TEntity,TId}" />.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="useScopedProvider">
    ///     <see langword="true" /> to resolve from <see cref="DataIntegrationTest{TDbContext}.ScopedServiceProvider" />;
    ///     otherwise resolve from <see cref="DataIntegrationTest{TDbContext}.RootServiceProvider" />.
    /// </param>
    /// <returns>An instance of <see cref="IReadWriteRepositoryAsync{TEntity,TId}" />.</returns>
    [SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "The type name created ends with Async hence the name.")]
    protected IReadWriteRepositoryAsync<TEntity, TId> CreateReadWriteRepositoryAsync<TEntity, TId>(bool useScopedProvider = true) where TEntity : class, IHasId<TId> =>
        GetServiceProvider(useScopedProvider).GetRequiredService<IReadWriteRepositoryAsync<TEntity, TId>>();

    private IServiceProvider GetServiceProvider(bool useScopedProvider) => useScopedProvider ? ScopedServiceProvider : RootServiceProvider;
}
