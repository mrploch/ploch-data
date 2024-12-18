using System;
using System.Collections.Generic;
using Dawn;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.AppServices;
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
                                                                                { typeof(IReadWriteRepository<,>), typeof(ReadWriteRepository<,>) }
                                                                            };

    private static readonly Dictionary<Type, Type> RepositoryAsyncTypeMappings = new()
                                                                                 {
                                                                                     { typeof(IReadRepositoryAsync<,>), typeof(ReadRepositoryAsync<,>) },
                                                                                     { typeof(IWriteRepositoryAsync<,>), typeof(ReadWriteRepositoryAsync<,>) },
                                                                                     { typeof(IReadWriteRepositoryAsync<,>), typeof(ReadWriteRepositoryAsync<,>) }
                                                                                 };

    /// <summary>
    ///     Registers the repository types in the service collection using <c>AddScoped</c> method.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <returns>The same service collection.</returns>
    /// r
    public static IServiceCollection AddRepositories<TDbContext>(this IServiceCollection serviceCollection, IConfiguration? configuration = null)
        where TDbContext : DbContext
    {
        Guard.Argument(serviceCollection, nameof(serviceCollection)).NotNull();
        configuration ??= new ConfigurationBuilder().Build();

        return AddRepositories<TDbContext>(serviceCollection, configuration, static (collection, sourceType, targetType) => collection.AddScoped(sourceType, targetType));
    }

    /// <summary>
    ///     Registers the repository types in the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="registrationFunction">The <see cref="IServiceCollection" /> method used for registration.</param>
    /// <typeparam name="TDbContext">The type of DbContext.</typeparam>
    /// <returns>The same service collection.</returns>
    private static IServiceCollection AddRepositories<TDbContext>(this IServiceCollection serviceCollection,
                                                                  IConfiguration configuration,
                                                                  Func<IServiceCollection, Type, Type, IServiceCollection> registrationFunction)
        where TDbContext : DbContext
    {
        Guard.Argument(serviceCollection, nameof(serviceCollection)).NotNull();
        Guard.Argument(registrationFunction, nameof(registrationFunction)).NotNull();

        serviceCollection.AddTransient<DbContext>(static provider => provider.GetRequiredService<TDbContext>())
                         .AddSingleton<IAuditEntityHandler, AuditEntityHandler>()
                         .AddSingleton<IUserInfoProvider, NullUserInfoProvider>();

        serviceCollection.Configure<RepositoriesConfiguration>(configuration.GetSection(nameof(RepositoriesConfiguration)));
        registrationFunction(serviceCollection, typeof(IQueryableRepository<>), typeof(QueryableRepository<>));
        registrationFunction(serviceCollection, typeof(IReadRepository<>), typeof(ReadRepository<>));
        registrationFunction(serviceCollection, typeof(IReadRepositoryAsync<>), typeof(ReadRepositoryAsync<>));

        foreach (var (sourceType, targetType) in RepositoryTypeMappings)
        {
            registrationFunction(serviceCollection, sourceType, targetType);
        }

        foreach (var (sourceType, targetType) in RepositoryAsyncTypeMappings)
        {
            registrationFunction(serviceCollection, sourceType, targetType);
        }

        serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();

        return serviceCollection;
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
    /// <param name="serviceCollection">The service collection to add the repositories to.</param>
    /// <param name="registrationFunction">The <see cref="IServiceCollection" /> method used for registration.</param>
    /// <typeparam name="TRepositoryInterface">The interface type of the custom repository.</typeparam>
    /// <typeparam name="TRepository">The implementation type of the custom repository.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The entity <c>Id</c> property type.</typeparam>
    /// <returns>
    ///     The same <see cref="IServiceCollection" /> passed in <paramref name="serviceCollection" /> used for method
    ///     chaining.
    /// </returns>
    public static IServiceCollection AddCustomReadWriteAsyncRepository<TRepositoryInterface, TRepository, TEntity, TId>(this IServiceCollection serviceCollection,
                                                                                                                        Func<IServiceCollection, Type, Type,
                                                                                                                                IServiceCollection>?
                                                                                                                            registrationFunction =
                                                                                                                            null)
        where TRepositoryInterface : class, IReadWriteRepositoryAsync<TEntity, TId>
        where TRepository : class, TRepositoryInterface, IReadWriteRepositoryAsync<TEntity, TId>
        where TEntity : class, IHasId<TId>
    {
        Guard.Argument(serviceCollection, nameof(serviceCollection)).NotNull();

        registrationFunction ??= static (collection, sourceType, targetType) => collection.AddScoped(sourceType, targetType);

        registrationFunction(serviceCollection, typeof(IQueryableRepository<TEntity>), typeof(TRepository));
        registrationFunction(serviceCollection, typeof(IReadRepositoryAsync<TEntity>), typeof(TRepository));

        foreach (var (sourceType, _) in RepositoryAsyncTypeMappings)
        {
            registrationFunction(serviceCollection, sourceType.MakeGenericType(typeof(TEntity), typeof(TId)), typeof(TRepository));
        }

        registrationFunction(serviceCollection, typeof(TRepositoryInterface), typeof(TRepository));

        return serviceCollection;
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
    /// <param name="serviceCollection">The service collection to add the repositories to.</param>
    /// <param name="registrationFunction">The <see cref="IServiceCollection" /> method used for registration.</param>
    /// <typeparam name="TRepositoryInterface">The interface type of the custom repository.</typeparam>
    /// <typeparam name="TRepository">The implementation type of the custom repository.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The entity <c>Id</c> property type.</typeparam>
    /// <returns>
    ///     The same <see cref="IServiceCollection" /> passed in <paramref name="serviceCollection" /> used for method
    ///     chaining.
    /// </returns>
    public static IServiceCollection AddCustomReadWriteRepository<TRepositoryInterface, TRepository, TEntity, TId>(this IServiceCollection serviceCollection,
                                                                                                                   Func<IServiceCollection, Type, Type,
                                                                                                                       IServiceCollection> registrationFunction)
        where TRepositoryInterface : class, IReadWriteRepositoryAsync<TEntity, TId>
        where TRepository : class, TRepositoryInterface, IReadWriteRepositoryAsync<TEntity, TId>
        where TEntity : class, IHasId<TId>
    {
        Guard.Argument(serviceCollection, nameof(serviceCollection)).NotNull();
        Guard.Argument(registrationFunction, nameof(registrationFunction)).NotNull();

        registrationFunction(serviceCollection, typeof(IQueryableRepository<>).MakeGenericType(typeof(TEntity)), typeof(TRepository));

        registrationFunction(serviceCollection, typeof(IQueryableRepository<TEntity>), typeof(TRepository));
        registrationFunction(serviceCollection, typeof(IReadRepository<TEntity>), typeof(TRepository));

        foreach (var repositoryTypeMapping in RepositoryTypeMappings)
        {
            registrationFunction(serviceCollection, repositoryTypeMapping.Key.MakeGenericType(typeof(TEntity), typeof(TId)), typeof(TRepository));
        }

        registrationFunction(serviceCollection, typeof(TRepositoryInterface), typeof(TRepository));

        return serviceCollection;
    }
}
