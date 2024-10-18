using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.IntegrationTesting;

public static class DbContextServicesRegistrationHelper
{
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
        var dbConnection = testDbContext.Database.GetDbConnection();

        testDbContext.Database.OpenConnection();


        var ensureCreated = testDbContext.Database.EnsureCreated();

        return (scope.ServiceProvider, testDbContext);
    }
}