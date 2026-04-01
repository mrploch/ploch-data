using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.SqlServer;

/// <summary>
///     Extension methods for registering a <see cref="DefaultDbContextCreationLifecycle" />
///     and SQL Server-configured <see cref="DbContext" /> instances in an
///     <see cref="IServiceCollection" />.
/// </summary>
public static class SqlServerDbContextCreationLifecycleServiceCollectionExtensions
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
    ///         <c>DefaultConnection</c> key via <see cref="ConnectionString.FromJsonFile()" />.
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
    public static IServiceCollection AddDbContextWithSqlServerCreationLifecycle<TDbContext>(
        this IServiceCollection services,
        Func<string?>? connectionString = null) where TDbContext : DbContext
    {
        connectionString ??= ConnectionString.FromJsonFile();

        return services.AddDefaultDbContextCreationLifecycle()
                       .AddDbContext<TDbContext>(options => options.UseSqlServer(connectionString() ??
                                                                                throw new InvalidOperationException("Connection string not found.")));
    }
}
