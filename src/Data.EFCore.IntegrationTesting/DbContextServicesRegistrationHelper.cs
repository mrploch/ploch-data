using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.IntegrationTesting;

/// <summary>
///     Helper class for registering and configuring DbContext services for integration testing.
/// </summary>
public static class DbContextServicesRegistrationHelper
{
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
        // Create the connection once and share it across all DbContext instances.
        // This is critical for SQLite in-memory databases: each new connection to :memory:
        // creates a separate empty database, so all consumers must share a single connection.
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        serviceCollection.AddSingleton(connection);
        serviceCollection.AddDbContext<TDbContext>(builder => builder.UseSqlite(connection));

        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection);
    }

    /// <summary>
    ///     Builds a DbContext and IServiceProvider for integration testing using a custom DbContext configurator.
    /// </summary>
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
        serviceCollection.AddDbContext<TDbContext>(dbContextConfigurator.Configure);

        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection);
    }

    private static (IServiceProvider RootProvider, IServiceProvider ScopedProvider, TDbContext DbContext)
        CreateProviderAndPrepareDbContext<TDbContext>(IServiceCollection serviceCollection) where TDbContext : DbContext
    {
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var testDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        testDbContext.Database.OpenConnection();
        testDbContext.Database.EnsureCreated();

        // Return the scoped service provider so that repositories resolved by tests
        // share the same DbContext instance (and its change tracker).
        // The shared connection in SqLiteDbContextConfigurator ensures all DbContext instances
        // (including those in UnitOfWork child scopes) access the same in-memory database.
        return (serviceProvider, scope.ServiceProvider, testDbContext);
    }
}
