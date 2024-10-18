using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides read operations for a collection of a <typeparamref name="TEntity" />.
/// </summary>
/// <inheritdoc />
public interface IReadRepository<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Gets the entity with the specified primary key values.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <returns>The entity found, or null.</returns>
    TEntity? GetById(object[] keyValues);

    /// <summary>
    ///     Finds the first entity that matches the specified query.
    /// </summary>
    /// <param name="query">The query to filter entities.</param>
    /// <param name="onDbSet">Optional function to customize the query on the DbSet.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The first entity that matches the query, or null if none found.</returns>
    TEntity? FindFirst(Expression<Func<TEntity, bool>> query,
                       Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                       CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all entities from the repository.
    /// </summary>
    /// <param name="onDbSet">Action to perform on DbSet on the query - for example Include.</param>
    /// <returns>A list of all entities.</returns>
    IList<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);

    /// <summary>
    ///     Gets a page of entities from the repository.
    /// </summary>
    /// <param name="pageNumber">The number of the page to get.</param>
    /// <param name="pageSize">The size of the page to get.</param>
    /// <param name="onDbSet">Action to perform on DbSet on the query - for example Include.</param>
    /// <returns>A list of entities for the specified page.</returns>
    IList<TEntity> GetPage(int pageNumber, int pageSize, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);

    /// <summary>
    ///     Gets the count of entities in the repository.
    /// </summary>
    /// <returns>The count of entities.</returns>
    int Count();
}

/// <summary>
///     Defines a repository that provides read operations for a collection of <typeparamref name="TEntity" /> with a
///     specified
///     identifier type.
/// </summary>
/// <inheritdoc />
public interface IReadRepository<TEntity, in TId> : IReadRepository<TEntity>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Gets the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to be found.</param>
    /// <param name="onDbSet">Action to perform on DbSet on the query - for example Include.</param>
    /// <returns>The entity found, or null.</returns>
    TEntity? GetById(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);
}