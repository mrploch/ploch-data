using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore;
using Ploch.Data.EFCore.SqLite;
using Ploch.Data.GenericRepository.EFCore;

// This is intentionally in the Data namespace so that when changing the database provider,
// only the project/package reference changes — no code modifications are needed.
// ReSharper disable once CheckNamespace
namespace Ploch.Data.SampleApp.Data;

/// <summary>
///     Provides SQLite-specific extension methods for registering the
///     <see cref="SampleAppDbContext" /> with the dependency injection container.
/// </summary>
public static class ServiceCollectionRegistrations
{
    /// <summary>
    ///     Registers the <see cref="SampleAppDbContext" /> using the SQLite provider,
    ///     the <see cref="SqLiteDbContextCreationLifecycle" />, and the generic
    ///     repository and Unit of Work services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">
    ///     A function that returns the SQLite connection string, or <c>null</c> to load
    ///     from <c>appsettings.json</c>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddDbContextWithRepositories<TDbContext>(
        this IServiceCollection services,
        Func<string?>? connectionString = null) where TDbContext : DbContext
    {
        connectionString ??= ConnectionString.FromJsonFile();

        return services.AddSingleton<IDbContextCreationLifecycle, SqLiteDbContextCreationLifecycle>()
                       .AddDbContext<TDbContext>(options => options.UseSqlite(connectionString() ??
                                                                             throw new InvalidOperationException("Connection string not found")))
                       .AddRepositories<TDbContext>();
    }
}
