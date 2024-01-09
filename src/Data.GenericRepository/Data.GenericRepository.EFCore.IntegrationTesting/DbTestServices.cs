using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

public class DbTestServices<TDbContext> : IDisposable
    where TDbContext : DbContext
{
    private readonly Action<IServiceCollection>? _configureServices;

    public DbTestServices(IDbContextConfigurator dbContextConfigurator, Action<IServiceCollection>? configureServices = null)
    {
        ServiceProvider = BuildServiceProvider(dbContextConfigurator, configureServices);
        DbContext = BuildDbContext();
    }

    public TDbContext DbContext { get; }

    public IServiceProvider ServiceProvider { get; }

    private IServiceProvider BuildServiceProvider(IDbContextConfigurator dbContextConfigurator, Action<IServiceCollection>? configureServices = null)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureDbContextServices(serviceCollection, dbContextConfigurator);
        configureServices?.Invoke(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    private TDbContext BuildDbContext()
    {
        var dbContext = ServiceProvider.GetRequiredService<TDbContext>();
        dbContext.Database.EnsureCreated();

        return dbContext;
    }

    protected virtual void ConfigureDbContextServices(IServiceCollection serviceCollection, IDbContextConfigurator dbContextConfigurator)
    {
        serviceCollection.AddDbContext<TDbContext>(dbContextConfigurator.Configure);
        serviceCollection.AddRepositories<TDbContext>();
    }

    /*public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (ServiceProvider is IDisposable disposableServiceProvider)
        {
            disposableServiceProvider.Dispose();
        }
    }*/
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposableServiceProvider)
        {
            disposableServiceProvider.Dispose();
        }
    }
}