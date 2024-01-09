using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

public static class RepositoryServicesRegistrationHelper
{
    public static (IServiceProvider, TDbContext) RegisterRepositoryServices<TDbContext>(IServiceCollection serviceCollection,
                                                                                        string connectionString = "Filename=:memory:")
        where TDbContext : DbContext
    {
        serviceCollection.AddDbContext<TDbContext>(builder =>
                                                   {
                                                       var connection = new SqliteConnection(connectionString);
                                                       connection.Open();
                                                       builder.UseSqlite(connection);
                                                   });
        serviceCollection.AddRepositories<TDbContext>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var testDbContext = serviceProvider.GetRequiredService<TDbContext>();
        testDbContext.Database.EnsureCreated();

        return (serviceProvider, testDbContext);
    }
}