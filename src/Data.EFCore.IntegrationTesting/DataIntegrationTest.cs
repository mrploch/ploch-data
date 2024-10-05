using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.EFCore.SqLite;

namespace Ploch.Data.EFCore.IntegrationTesting;

public abstract class DataIntegrationTest<TDbContext> : IDisposable
    where TDbContext : DbContext
{
    protected DataIntegrationTest(IDbContextConfigurator? dbContextConfigurator = null)
    {
        var serviceCollection = new ServiceCollection();

        // ReSharper disable once VirtualMemberCallInConstructor - this is not a problem here
        ConfigureServices(serviceCollection);

        (ServiceProvider, DbContext) =
            DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TDbContext>(serviceCollection,
                                                                                             dbContextConfigurator ??
                                                                                             new SqLiteDbContextConfigurator(SqLiteConnectionOptions.InMemory));
    }

    protected TDbContext DbContext { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected virtual void ConfigureServices(IServiceCollection services)
    { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (ServiceProvider is ServiceProvider sp)
        {
            sp.Dispose();
        }
    }
}