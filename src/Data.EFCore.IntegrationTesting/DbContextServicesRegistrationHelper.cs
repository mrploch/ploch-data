using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.IntegrationTesting;

/// <summary>
///     Helper class for registering and configuring DbContext services for integration testing.
/// </summary>
public static class DbContextServicesRegistrationHelper
{
    /// <summary>
    ///     Builds a <see cref="TestDbContextHarness{TDbContext}" /> that owns the root service provider, the initial
    ///     scope, the shared SQLite connection and the prepared database context.
    /// </summary>
    /// <remarks>
    ///     Prefer this over <see cref="BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,string)" />: the
    ///     harness is a single <see cref="IDisposable" /> that releases every one of those resources, whereas the tuple
    ///     leaves ownership to the caller.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="connectionString">The database connection string. Default is an in-memory SQLite database.</param>
    /// <returns>A harness owning every resource created for the test.</returns>
    public static TestDbContextHarness<TDbContext> BuildHarness<TDbContext>(IServiceCollection serviceCollection,
                                                                           string connectionString = "Data Source=:memory:") where TDbContext : DbContext
    {
        // Create the connection once and share it across all DbContext instances.
        // This is critical for SQLite in-memory databases: each new connection to :memory:
        // creates a separate empty database, so all consumers must share a single connection.
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        serviceCollection.AddSingleton(connection);
        serviceCollection.AddDbContext<TDbContext>(builder => builder.UseSqlite(connection));

        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection, connection);
    }

    /// <summary>
    ///     Builds a <see cref="TestDbContextHarness{TDbContext}" /> using a custom DbContext configurator.
    /// </summary>
    /// <remarks>
    ///     Prefer this over
    ///     <see cref="BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,IDbContextConfigurator)" />: the
    ///     harness is a single <see cref="IDisposable" /> that releases the root provider, the initial scope and the
    ///     database context, whereas the tuple leaves ownership to the caller.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="dbContextConfigurator">The configurator responsible for setting up the DbContext options.</param>
    /// <returns>A harness owning every resource created for the test.</returns>
    public static TestDbContextHarness<TDbContext> BuildHarness<TDbContext>(IServiceCollection serviceCollection,
                                                                           IDbContextConfigurator dbContextConfigurator) where TDbContext : DbContext
    {
        serviceCollection.AddDbContext<TDbContext>(dbContextConfigurator.Configure);
        serviceCollection.AddDbContextFactory<TDbContext>(dbContextConfigurator.Configure);

        // The connection, if any, belongs to the configurator, so the harness must not dispose it.
        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection, connection: null);
    }

    /// <inheritdoc cref="BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,IDbContextConfigurator)" />
    /// <summary>
    ///     Builds a DbContext and IServiceProvider for integration testing.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="connectionString">The database connection string. Default is an in-memory SQLite database.</param>
    public static (IServiceProvider RootProvider, IServiceProvider ScopedProvider, TDbContext DbContext) BuildDbContextAndServiceProvider<TDbContext>(
        IServiceCollection serviceCollection,
        string connectionString = "Data Source=:memory:") where TDbContext : DbContext
    {
        var (rootProvider, scopedProvider, dbContext) = BuildHarness<TDbContext>(serviceCollection, connectionString);

        return (rootProvider, scopedProvider, dbContext);
    }

    /// <summary>
    ///     Builds a DbContext and IServiceProvider for integration testing using a custom DbContext configurator.
    /// </summary>
    /// <remarks>
    ///     This overload hands back three references but no ownership: the caller remains responsible for disposing the
    ///     root provider, the scope behind <c>ScopedProvider</c> and the returned context. Prefer
    ///     <see cref="BuildHarness{TDbContext}(IServiceCollection,IDbContextConfigurator)" />, which owns all of them.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="dbContextConfigurator">The configurator responsible for setting up the DbContext options.</param>
    /// <returns>
    ///     A tuple containing the root IServiceProvider (<c>RootProvider</c>), the scoped IServiceProvider (<c>ScopedProvider</c>), the configured TDbContext (
    ///     <c>DbContext</c>).
    /// </returns>
    public static (IServiceProvider RootProvider, IServiceProvider ScopedProvider, TDbContext DbContext) BuildDbContextAndServiceProvider<TDbContext>(
        IServiceCollection serviceCollection,
        IDbContextConfigurator dbContextConfigurator) where TDbContext : DbContext
    {
        var (rootProvider, scopedProvider, dbContext) = BuildHarness<TDbContext>(serviceCollection, dbContextConfigurator);

        return (rootProvider, scopedProvider, dbContext);
    }

    private static TestDbContextHarness<TDbContext> CreateProviderAndPrepareDbContext<TDbContext>(IServiceCollection serviceCollection, SqliteConnection? connection)
        where TDbContext : DbContext
    {
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var testDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        testDbContext.Database.OpenConnection();
        testDbContext.Database.EnsureCreated();

        // The harness exposes the scoped service provider so that repositories resolved by tests
        // share the same DbContext instance (and its change tracker).
        // The shared connection in SqLiteDbContextConfigurator ensures all DbContext instances
        // (including those in UnitOfWork child scopes) access the same in-memory database.
        return new TestDbContextHarness<TDbContext>(serviceProvider, scope, testDbContext, connection);
    }
}
