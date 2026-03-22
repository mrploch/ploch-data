using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository.EFCore;

namespace Ploch.Data.SampleApp.Data;

public static class ServiceCollectionRegistrations
{
    public static IServiceCollection AddSampleAppDataServices(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        return services
            .AddDbContext<SampleAppDbContext>(configureOptions)
            .AddRepositories<SampleAppDbContext>();
    }
}
