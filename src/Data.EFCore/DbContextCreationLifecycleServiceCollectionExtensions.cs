using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore;

/// <summary>
///     Extension methods for registering <see cref="IDbContextCreationLifecycle" />
///     implementations in an <see cref="IServiceCollection" />.
/// </summary>
public static class DbContextCreationLifecycleServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the <see cref="DefaultDbContextCreationLifecycle" /> as a singleton
    ///     implementation of <see cref="IDbContextCreationLifecycle" />.
    /// </summary>
    /// <param name="services">The service collection to add the registration to.</param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddDefaultDbContextCreationLifecycle(this IServiceCollection services)
    {
        return services.AddSingleton<IDbContextCreationLifecycle, DefaultDbContextCreationLifecycle>();
    }

    /// <summary>
    ///     Registers the <see cref="DefaultDbContextCreationLifecycle" /> and a
    ///     <typeparamref name="TDbContext" /> with the specified options configuration.
    /// </summary>
    /// <remarks>
    ///     This is a convenience method that combines
    ///     <see cref="AddDefaultDbContextCreationLifecycle" /> with
    ///     <see cref="EntityFrameworkServiceCollectionExtensions.AddDbContext{TContext}(IServiceCollection, Action{DbContextOptionsBuilder}, ServiceLifetime, ServiceLifetime)" />.
    ///     It does <b>not</b> register generic repositories — call
    ///     <c>AddRepositories&lt;TDbContext&gt;()</c> separately if needed.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to register.</typeparam>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configureOptions">
    ///     An action to configure the <see cref="DbContextOptionsBuilder" /> for the context.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddDbContextWithDefaultCreationLifecycle<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions) where TDbContext : DbContext
    {
        return services.AddDefaultDbContextCreationLifecycle()
                       .AddDbContext<TDbContext>(configureOptions);
    }
}
