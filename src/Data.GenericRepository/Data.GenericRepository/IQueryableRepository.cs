using System;
using System.Linq;
using System.Linq.Expressions;

namespace Ploch.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides queryable access to a collection of a <typeparamref name="TEntity" />.
/// </summary>
/// <typeparam name="TEntity">The entity type for this repository.</typeparam>
public interface IQueryableRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Gets a queryable collection of all entities.
    /// </summary>
    IQueryable<TEntity> Entities { get; }

    /// <summary>
    ///     Gets a queryable collection of entities for a specific page.
    /// </summary>
    /// <param name="pageNumber">The number of the page to get.</param>
    /// <param name="pageSize">The size of the page to get.</param>
    /// <param name="sortBy">Sort by property selector.</param>
    /// <param name="query">A LINQ expression to filter the entities.</param>
    /// <param name="onDbSet">
    ///     An optional function used to <em>shape</em> the query — for example eager loading with
    ///     <c>Include</c> / <c>ThenInclude</c>, ordering, or <c>AsNoTracking</c>. Do not filter here; express
    ///     filtering with <paramref name="query" /> instead.
    /// </param>
    /// <returns>A queryable collection of entities for the specified page.</returns>
    IQueryable<TEntity> GetPageQuery(int pageNumber,
                                     int pageSize,
                                     Expression<Func<TEntity, object>>? sortBy = null,
                                     Expression<Func<TEntity, bool>>? query = null,
                                     Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);
}
