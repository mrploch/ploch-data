using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.SqlServer;

/// <summary>
///     Extension methods for registering a <see cref="DefaultDbContextCreationLifecycle" />
///     and SQL Server-configured <see cref="DbContext" /> instances in an
///     <see cref="IServiceCollection" />.
/// </summary>
/// <example>
///     Using the default connection string from appsettings.json:
///     <code>
///         services.AddDbContextUsingSqlServer&lt;MyDbContext&gt;();
///     </code>
///     Using a custom connection string function:
///     <code>
///         services.AddDbContextUsingSqlServer&lt;MyDbContext&gt;(() => "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
///     </code>
///     Using a custom configurator:
///     <code>
///         var configurator = new SqlServerDbContextConfigurator("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
///         services.AddDbContextUsingSqlServer&lt;MyDbContext&gt;(configurator);
///     </code>
/// </example>
public static class DbContextUsingSqlServerRegistrations
{
    /// <summary>
    ///     Registers the <see cref="DefaultDbContextCreationLifecycle" /> and a
    ///     <typeparamref name="TDbContext" /> configured to use SQL Server.
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
    ///         <c>DefaultConnection</c> key via <c>ConnectionString.FromJsonFile()</c>.
    ///     </para>
    ///     <para>
    ///         This method does <b>not</b> register generic repositories — call
    ///         <c>AddRepositories&lt;TDbContext&gt;()</c> separately if needed.
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
    public static IServiceCollection AddDbContextUsingSqlServer<TDbContext>(this IServiceCollection services, Func<string?>? connectionString = null) where TDbContext : DbContext
    {
        connectionString ??= ConnectionString.FromJsonFile();
        var resolvedConnectionString = connectionString() ??
                                       throw new InvalidOperationException($"SQL Server connection string for {typeof(TDbContext).Name} not found. " +
                                                                           "Provide a connection string or ensure it is present in appsettings.json under 'ConnectionStrings:DefaultConnection'.");

        return services.AddDbContextUsingSqlServer<TDbContext>(new SqlServerDbContextConfigurator(resolvedConnectionString));
    }

    /// <summary>
    ///     Registers the <see cref="DefaultDbContextCreationLifecycle" /> and a
    ///     <typeparamref name="TDbContext" /> using the provided <paramref name="configurator" />.
    /// </summary>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to register.</typeparam>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configurator">
    ///     The configurator used to configure the <typeparamref name="TDbContext" /> with
    ///     SQL Server.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddDbContextUsingSqlServer<TDbContext>(this IServiceCollection services, SqlServerDbContextConfigurator configurator)
        where TDbContext : DbContext => services.AddSingleton<IDbContextCreationLifecycle, DefaultDbContextCreationLifecycle>().AddDbContext<TDbContext>(configurator.Configure);
}
