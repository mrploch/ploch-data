using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ploch.Common.AppServices.Security;
using Ploch.Common.ArgumentChecking;
using Ploch.Data.EFCore;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides extension methods for registering repository types in the <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionRegistration
{
    private static readonly Dictionary<Type, Type> RepositoryTypeMappings = new()
    {
        { typeof(IReadRepository<,>), typeof(ReadRepository<,>) },
        { typeof(IWriteRepository<,>), typeof(ReadWriteRepository<,>) },
        { typeof(IReadWriteRepository<,>), typeof(ReadWriteRepository<,>) },
    };

    private static readonly Dictionary<Type, Type> RepositoryAsyncTypeMappings = new()
    {
        { typeof(IReadRepositoryAsync<,>), typeof(ReadRepositoryAsync<,>) },
        { typeof(IWriteRepositoryAsync<,>), typeof(ReadWriteRepositoryAsync<,>) },
        { typeof(IReadWriteRepositoryAsync<,>), typeof(ReadWriteRepositoryAsync<,>) },
    };

    /// <summary>
    ///     Registers a <typeparamref name="TDbContext" /> using the <see cref="IDbContextConfigurator" />,
    ///     using a default type of the <see cref="IDbContextCreationLifecycle" /> - <see cref="DefaultDbContextCreationLifecycle" />, and the generic
    ///     repository and Unit of Work services.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configurator">The configurator responsible for setting up the DbContext options.</param>
    /// <param name="configuration">Optional app configuration used for configuring of the Generic Repositories.</param>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to register.</typeparam>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddDbContextWithRepositories<TDbContext>(this IServiceCollection services,
                                                                              IDbContextConfigurator configurator,
                                                                              IConfiguration? configuration = null) where TDbContext : DbContext =>
        services.AddDbContextWithRepositories<TDbContext, DefaultDbContextCreationLifecycle>(configurator, configuration);

    /// <summary>
    ///     Registers a <typeparamref name="TDbContext" /> using the <see cref="IDbContextConfigurator" />,
    ///     allowing to specify which <see cref="IDbContextCreationLifecycle" /> to use, and the generic
    ///     repository and Unit of Work services.
    /// </summary>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to register.</typeparam>
    /// <typeparam name="TDbContextCreationLifecycle">The type of <see cref="IDbContextCreationLifecycle" /> to register.</typeparam>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="configurator">The configurator responsible for setting up the DbContext options.</param>
    /// <param name="configuration">Optional app configuration used for configuring of the Generic Repositories.</param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddDbContextWithRepositories<TDbContext, TDbContextCreationLifecycle>(this IServiceCollection services,
                                                                                                           IDbContextConfigurator configurator,
                                                                                                           IConfiguration? configuration = null)
        where TDbContext : DbContext where TDbContextCreationLifecycle : class, IDbContextCreationLifecycle =>
        services.AddDbContextWithRepositories<TDbContext, TDbContextCreationLifecycle>(configurator.NotNull().Configure, configuration);

    /// <summary>
    ///     Registers a <typeparamref name="TDbContext" /> using the <see cref="DbContextOptionsBuilder" />,
    ///     using a default type of the <see cref="IDbContextCreationLifecycle" /> - <see cref="DefaultDbContextCreationLifecycle" />, and the generic
    ///     repository and Unit of Work services.
    /// </summary>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="options">The options to pass to <see cref="DbContextOptionsBuilder" />.</param>
    /// <param name="configuration">Optional app configuration used for configuring of the Generic Repositories.</param>
    /// <remarks>
    ///     <para>
    ///         SQL Server does not require any special model-creation lifecycle logic,
    ///         so this method registers the <see cref="DefaultDbContextCreationLifecycle" />
    ///         (no-op) implementation.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to register.</typeparam>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="options" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddDbContextWithRepositories<TDbContext>(this IServiceCollection services,
                                                                              Action<DbContextOptionsBuilder> options,
                                                                              IConfiguration? configuration = null) where TDbContext : DbContext =>
        services.AddDbContextWithRepositories<TDbContext, DefaultDbContextCreationLifecycle>(options, configuration);

    /// <summary>
    ///     Registers a <typeparamref name="TDbContext" /> using the <see cref="DbContextOptionsBuilder" />,
    ///     allowing to specify which <see cref="IDbContextCreationLifecycle" /> to use, and the generic
    ///     repository and Unit of Work services.
    /// </summary>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext" /> to register.</typeparam>
    /// <typeparam name="TDbContextCreationLifecycle">The type of <see cref="IDbContextCreationLifecycle" /> to register.</typeparam>
    /// <param name="services">The service collection to add the registrations to.</param>
    /// <param name="options">The options to pass to <see cref="DbContextOptionsBuilder" />.</param>
    /// <param name="configuration">The app configuration used for configuring of the Generic Repositories.</param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="options" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddDbContextWithRepositories<TDbContext, TDbContextCreationLifecycle>(this IServiceCollection services,
                                                                                                           Action<DbContextOptionsBuilder> options,
                                                                                                           IConfiguration? configuration = null)
        where TDbContext : DbContext where TDbContextCreationLifecycle : class, IDbContextCreationLifecycle => services.NotNull()
                                                                                                                       .AddSingleton<IDbContextCreationLifecycle,
                                                                                                                           TDbContextCreationLifecycle>()
                                                                                                                       .AddDbContext<TDbContext>(options.NotNull())
                                                                                                                       .AddRepositories<TDbContext>(configuration);

    /// <summary>
    ///     Registers the repository types in the service collection using <c>AddScoped</c> method.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddRepositories<TDbContext>(this IServiceCollection services, IConfiguration? configuration = null) where TDbContext : DbContext
    {
        configuration ??= new ConfigurationBuilder().Build();

        return services.NotNull().AddRepositories<TDbContext>(configuration, static (collection, sourceType, targetType) => collection.AddScoped(sourceType, targetType));
    }

    /// <summary>
    ///     Registers a mapping of read / write async repository interfaces to custom repository type in the service
    ///     collection.
    /// </summary>
    /// <remarks>
    ///     This method registers the following mappings:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <typeparamref name="TRepositoryInterface" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="IReadWriteRepositoryAsync{TEntity,TId}" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="IWriteRepositoryAsync{TEntity,TId}" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="IReadRepositoryAsync{TEntity,TId}" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///     </list>
    ///     It allows to specify how the <typeparamref name="TRepository" /> is registered in the service collection using
    ///     the <paramref name="registrationFunction" /> parameter.
    ///     <example>
    ///         Example below shows how to register the custom repository type using the
    ///         <see cref="IServiceCollection" /> <c>AddScoped</c> extension method:
    ///         <code>
    /// <![CDATA[
    ///     services.AddCustomAsyncRepository<ICarReadWriteRepository, CarReadWriteRepository, Car, int>
    ///                 ((collection,
    ///                 sourceType, targetType) => collection.AddScoped(sourceType, targetType));
    /// ]]>
    /// </code>
    ///     </example>
    /// </remarks>
    /// <param name="services">The service collection to add the repositories to.</param>
    /// <param name="registrationFunction">The <see cref="IServiceCollection" /> method used for registration.</param>
    /// <typeparam name="TRepositoryInterface">The interface type of the custom repository.</typeparam>
    /// <typeparam name="TRepository">The implementation type of the custom repository.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The entity <c>Id</c> property type.</typeparam>
    /// <returns>
    ///     The same <see cref="IServiceCollection" /> passed in <paramref name="services" /> used for method
    ///     chaining.
    /// </returns>
    public static IServiceCollection AddCustomReadWriteAsyncRepository<TRepositoryInterface, TRepository, TEntity, TId>(
        this IServiceCollection services,
        Func<IServiceCollection, Type, Type, IServiceCollection>? registrationFunction = null) where TRepositoryInterface : class, IReadWriteRepositoryAsync<TEntity, TId>
                                                                                               where TRepository : class, TRepositoryInterface,
                                                                                               IReadWriteRepositoryAsync<TEntity, TId>
                                                                                               where TEntity : class, IHasId<TId>
    {
        services.NotNull();

        registrationFunction ??= static (collection, sourceType, targetType) => collection.AddScoped(sourceType, targetType);

        registrationFunction(services, typeof(IQueryableRepository<TEntity>), typeof(TRepository));
        registrationFunction(services, typeof(IReadRepositoryAsync<TEntity>), typeof(TRepository));

        foreach (var (sourceType, _) in RepositoryAsyncTypeMappings)
        {
            registrationFunction(services, sourceType.MakeGenericType(typeof(TEntity), typeof(TId)), typeof(TRepository));
        }

        registrationFunction(services, typeof(TRepositoryInterface), typeof(TRepository));

        return services;
    }

    /// <summary>
    ///     Registers a mapping of read / write async repository interfaces to custom repository type in the service
    ///     collection.
    /// </summary>
    /// <remarks>
    ///     This method registers the following mappings:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <typeparamref name="TRepositoryInterface" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="IReadWriteRepositoryAsync{TEntity,TId}" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="IWriteRepositoryAsync{TEntity,TId}" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="IReadRepositoryAsync{TEntity,TId}" /> to <typeparamref name="TRepository" />
    ///             </description>
    ///         </item>
    ///     </list>
    ///     It allows to specify how the <typeparamref name="TRepository" /> is registered in the service collection using
    ///     the <paramref name="registrationFunction" /> parameter.
    ///     <example>
    ///         Example below shows how to register the custom repository type using the
    ///         <see cref="IServiceCollection" /> <c>AddScoped</c> extension method:
    ///         <code>
    /// <![CDATA[
    ///     services.AddCustomAsyncRepository<ICarReadWriteRepository, CarReadWriteRepository, Car, int>
    ///                 ((collection,
    ///                 sourceType, targetType) => collection.AddScoped(sourceType, targetType));
    /// ]]>
    /// </code>
    ///     </example>
    /// </remarks>
    /// <param name="services">The service collection to add the repositories to.</param>
    /// <param name="registrationFunction">The <see cref="IServiceCollection" /> method used for registration.</param>
    /// <typeparam name="TRepositoryInterface">The interface type of the custom repository.</typeparam>
    /// <typeparam name="TRepository">The implementation type of the custom repository.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The entity <c>Id</c> property type.</typeparam>
    /// <returns>
    ///     The same <see cref="IServiceCollection" /> passed in <paramref name="services" /> used for method
    ///     chaining.
    /// </returns>
    public static IServiceCollection AddCustomReadWriteRepository<TRepositoryInterface, TRepository, TEntity, TId>(this IServiceCollection services,
                                                                                                                   Func<IServiceCollection, Type, Type, IServiceCollection>
                                                                                                                       registrationFunction)
        where TRepositoryInterface : class, IReadWriteRepositoryAsync<TEntity, TId>
        where TRepository : class, TRepositoryInterface, IReadWriteRepositoryAsync<TEntity, TId>
        where TEntity : class, IHasId<TId>
    {
        services.NotNull();
        registrationFunction.NotNull();

        registrationFunction(services, typeof(IQueryableRepository<>).MakeGenericType(typeof(TEntity)), typeof(TRepository));

        registrationFunction(services, typeof(IQueryableRepository<TEntity>), typeof(TRepository));
        registrationFunction(services, typeof(IReadRepository<TEntity>), typeof(TRepository));

        foreach (var repositoryTypeMapping in RepositoryTypeMappings)
        {
            registrationFunction(services, repositoryTypeMapping.Key.MakeGenericType(typeof(TEntity), typeof(TId)), typeof(TRepository));
        }

        registrationFunction(services, typeof(TRepositoryInterface), typeof(TRepository));

        return services;
    }

    /// <summary>
    ///     Registers the repository types in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="registrationFunction">The <see cref="IServiceCollection" /> method used for registration.</param>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <returns>The same service collection.</returns>
    private static IServiceCollection AddRepositories<TDbContext>(this IServiceCollection services,
                                                                  IConfiguration configuration,
                                                                  Func<IServiceCollection, Type, Type, IServiceCollection> registrationFunction) where TDbContext : DbContext
    {
        services.NotNull();
        registrationFunction.NotNull();

        services.AddTransient<DbContext>(static provider => provider.GetRequiredService<TDbContext>())
                .AddSingleton<IAuditEntityHandler, AuditEntityHandler>()
                .AddSingleton<IUserInfoProvider, NullUserInfoProvider>()
                .TryAddSingleton(TimeProvider.System);

        services.Configure<RepositoriesConfiguration>(configuration.GetSection(nameof(RepositoriesConfiguration)));
        registrationFunction(services, typeof(IQueryableRepository<>), typeof(QueryableRepository<>));
        registrationFunction(services, typeof(IReadRepository<>), typeof(ReadRepository<>));
        registrationFunction(services, typeof(IReadRepositoryAsync<>), typeof(ReadRepositoryAsync<>));

        foreach (var (sourceType, targetType) in RepositoryTypeMappings)
        {
            registrationFunction(services, sourceType, targetType);
        }

        foreach (var (sourceType, targetType) in RepositoryAsyncTypeMappings)
        {
            registrationFunction(services, sourceType, targetType);
        }

        services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();

        return services;
    }
}
