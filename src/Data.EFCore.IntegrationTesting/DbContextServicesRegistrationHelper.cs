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
    ///     Builds a DbContext and IServiceProvider for integration testing.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="connectionString">The database connection string. Default is in-memory SQLite database.</param>
    /// <returns>A tuple containing the IServiceProvider and the configured TDbContext.</returns>
    public static (IServiceProvider, TDbContext) BuildDbContextAndServiceProvider<TDbContext>(IServiceCollection serviceCollection,
                                                                                              string connectionString = "Filename=:memory:")
        where TDbContext : DbContext
    {
        serviceCollection.AddDbContext<TDbContext>(builder =>
                                                   {
                                                       var connection = new SqliteConnection(connectionString);
                                                       connection.Open();
                                                       builder.UseSqlite(connection);
                                                   });

        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection);
    }

    /// <summary>
    ///     Builds a DbContext and IServiceProvider for integration testing using a custom DbContext configurator.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="dbContextConfigurator">The configurator responsible for setting up the DbContext options.</param>
    /// <returns>A tuple containing the IServiceProvider and the configured TDbContext.</returns>
    public static (IServiceProvider, TDbContext) BuildDbContextAndServiceProvider<TDbContext>(IServiceCollection serviceCollection,
                                                                                              IDbContextConfigurator dbContextConfigurator)
        where TDbContext : DbContext
    {
        serviceCollection.AddDbContext<TDbContext>(dbContextConfigurator.Configure);

        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection);
    }

    private static (IServiceProvider, TDbContext) CreateProviderAndPrepareDbContext<TDbContext>(IServiceCollection serviceCollection)
        where TDbContext : DbContext
    {
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();
        var testDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        testDbContext.Database.OpenConnection();

        testDbContext.Database.EnsureCreated();

        return (scope.ServiceProvider, testDbContext);
    }
}
