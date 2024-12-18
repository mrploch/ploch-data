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
    /// <param name="onDbSet">Action to perform on DbSet on the query - for example, Include.</param>
    /// <returns>A queryable collection of entities for the specified page.</returns>
    IQueryable<TEntity> GetPageQuery(int pageNumber,
                                     int pageSize,
                                     Func<TEntity, object>? sortBy = null,
                                     Expression<Func<TEntity, bool>>? query = null,
                                     Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);
}
