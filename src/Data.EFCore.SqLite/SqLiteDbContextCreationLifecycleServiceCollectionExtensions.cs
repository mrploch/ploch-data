using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.SqLite;

/// <summary>
///     Extension methods for registering <see cref="SqLiteDbContextCreationLifecycle" />
///     and SQLite-configured <see cref="DbContext" /> instances in an
///     <see cref="IServiceCollection" />.
/// </summary>
public static class SqLiteDbContextCreationLifecycleServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the <see cref="SqLiteDbContextCreationLifecycle" /> as a singleton
    ///     implementation of <see cref="IDbContextCreationLifecycle" />.
    /// </summary>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddSqLiteDbContextCreationLifecycle(this IServiceCollection services)
    {
        return services.AddSingleton<IDbContextCreationLifecycle, SqLiteDbContextCreationLifecycle>();
    }

    /// <summary>
    ///     Registers the <see cref="SqLiteDbContextCreationLifecycle" /> and a
    ///     <typeparamref name="TDbContext" /> configured to use SQLite.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is a convenience method that combines
    ///         <see cref="AddSqLiteDbContextCreationLifecycle" /> with
    ///         <see cref="EntityFrameworkServiceCollectionExtensions.AddDbContext{TContext}(IServiceCollection, Action{DbContextOptionsBuilder}, ServiceLifetime, ServiceLifetime)" />
    ///         using the SQLite provider.
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
    ///     A function that returns the SQLite connection string, or <c>null</c> to load
    ///     from <c>appsettings.json</c>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the connection string function returns <c>null</c>.
    /// </exception>
    public static IServiceCollection AddDbContextWithSqLiteCreationLifecycle<TDbContext>(
        this IServiceCollection services,
        Func<string?>? connectionString = null) where TDbContext : DbContext
    {
        connectionString ??= ConnectionString.FromJsonFile();

        return services.AddSqLiteDbContextCreationLifecycle()
                       .AddDbContext<TDbContext>(options => options.UseSqlite(connectionString() ??
                                                                             throw new InvalidOperationException("Connection string not found.")));
    }
}
