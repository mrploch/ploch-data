using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore;

namespace Ploch.Data.GenericRepository.EFCore.DependencyInjection;

/// <summary>
///     Provides extension methods for registering a <see cref="DbContext" /> with the
///     SQL Server provider, the <see cref="DefaultDbContextCreationLifecycle" />, and
///     generic repository and Unit of Work services in a single call.
/// </summary>
/// <remarks>
///     <para>
///         This class and <c>Ploch.Data.GenericRepository.EFCore.DependencyInjection.SqLite</c>
///         share the same namespace and method signature. Switching the database provider
///         requires only changing the package reference — no code changes are needed.
///     </para>
///     <example>
///         <code>
///         // In Program.cs — identical regardless of which DI package is referenced:
///         builder.Services.AddDbContextWithRepositories&lt;MyDbContext&gt;();
///         </code>
///     </example>
/// </remarks>
public static class ServiceCollectionRegistrations
{
    /// <summary>
    ///     Registers a <typeparamref name="TDbContext" /> using the SQL Server provider,
    ///     the <see cref="DefaultDbContextCreationLifecycle" />, and the generic
    ///     repository and Unit of Work services.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         SQL Server does not require any special model-creation lifecycle logic,
    ///         so this method registers the <see cref="DefaultDbContextCreationLifecycle" />
    ///         (no-op) implementation.
    ///     </para>
    ///     <para>
    ///         If <paramref name="connectionString" /> is <c>null</c>, the connection
    ///         string is loaded from <c>appsettings.json</c> using the
    ///         <c>DefaultConnection</c> key via <see cref="ConnectionString.FromJsonFile()" />.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to register.</typeparam>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="connectionString">
    ///     A function that returns the SQL Server connection string, or <c>null</c> to
    ///     load from <c>appsettings.json</c>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the connection string function returns <c>null</c>.
    /// </exception>
    public static IServiceCollection AddDbContextWithRepositories<TDbContext>(
        this IServiceCollection services,
        Func<string?>? connectionString = null) where TDbContext : DbContext
    {
        connectionString ??= ConnectionString.FromJsonFile();

        return services.AddSingleton<IDbContextCreationLifecycle, DefaultDbContextCreationLifecycle>()
                       .AddDbContext<TDbContext>(options => options.UseSqlServer(connectionString() ??
                                                                                throw new InvalidOperationException("Connection string not found.")))
                       .AddRepositories<TDbContext>();
    }
}
