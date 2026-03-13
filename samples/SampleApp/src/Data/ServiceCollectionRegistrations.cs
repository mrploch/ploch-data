using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository.EFCore;

namespace Ploch.Data.SampleApp.Data;

public static class ServiceCollectionRegistrations
{
    public static IServiceCollection AddSampleAppDataServices(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions,
        IConfiguration? configuration = null)
    {
        configuration ??= new ConfigurationBuilder().Build();

        return services
            .AddDbContext<SampleAppDbContext>(configureOptions)
            .AddRepositories<SampleAppDbContext>(configuration);
    }
}
