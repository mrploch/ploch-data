using System.Collections.Generic;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides read operations for a collection of <see cref="TEntity" />.
/// </summary>
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
    ///     Gets all entities from the repository.
    /// </summary>
    /// <returns>A list of all entities.</returns>
    IList<TEntity> GetAll();

    /// <summary>
    ///     Gets a page of entities from the repository.
    /// </summary>
    /// <param name="pageNumber">The number of the page to get.</param>
    /// <param name="pageSize">The size of the page to get.</param>
    /// <returns>A list of entities for the specified page.</returns>
    IList<TEntity> GetPage(int pageNumber, int pageSize);

    /// <summary>
    ///     Gets the count of entities in the repository.
    /// </summary>
    /// <returns>The count of entities.</returns>
    int Count();
}

/// <summary>
///     Defines a repository that provides read operations for a collection of <see cref="TEntity" /> with a specified
///     identifier type.
/// </summary>
public interface IReadRepository<TEntity, in TId> : IReadRepository<TEntity>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Gets the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to be found.</param>
    /// <returns>The entity found, or null.</returns>
    TEntity? GetById(TId id);
}