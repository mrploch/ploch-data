using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.DependencyInjection;

namespace Ploch.Data.GenericRepository.EFCore.DependencyInjection;

public abstract class GenericRepositoriesServicesBundle<TDbContext> : ConfigurableServicesBundle where TDbContext : DbContext
{
    protected override void Configure(IConfiguration configuration)
    {
        Services.AddDbContext<TDbContext>(GetOptionsBuilderAction(configuration)).AddRepositories<TDbContext>(configuration);
    }

    protected abstract Action<DbContextOptionsBuilder> GetOptionsBuilderAction(IConfiguration? configuration);
}
