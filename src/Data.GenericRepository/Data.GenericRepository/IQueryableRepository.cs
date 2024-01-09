using System.Linq;

namespace Ploch.Common.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides queryable access to a collection of <see cref="TEntity" />.
/// </summary>
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
    /// <returns>A queryable collection of entities for the specified page.</returns>
    IQueryable<TEntity> GetPageQuery(int pageNumber, int pageSize);
}