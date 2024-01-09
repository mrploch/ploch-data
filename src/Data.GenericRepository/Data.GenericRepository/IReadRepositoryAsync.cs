using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides asynchronous read operations for a collection of <see cref="TEntity" />.
/// </summary>
public interface IReadRepositoryAsync<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Asynchronously gets the entity with the specified primary key values.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity found, or null.</returns>
    Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously gets all entities from the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of all entities.</returns>
    Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously gets a page of entities from the repository.
    /// </summary>
    /// <param name="pageNumber">The number of the page to get.</param>
    /// <param name="pageSize">The size of the page to get.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of entities for the
    ///     specified page.
    /// </returns>
    Task<IList<TEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously gets the count of entities in the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the count of entities.</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
///     Defines a repository that provides asynchronous read operations for a collection of <see cref="TEntity" /> with a
///     specified identifier type.
/// </summary>
public interface IReadRepositoryAsync<TEntity, in TId> : IReadRepositoryAsync<TEntity>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Asynchronously gets the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to be found.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity found, or null.</returns>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
}