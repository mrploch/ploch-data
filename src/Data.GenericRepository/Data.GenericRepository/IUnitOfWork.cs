using System;
using System.Threading;
using System.Threading.Tasks;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository;

/// <summary>
///     Defines a unit of work, which is a way to combine multiple repository operations into a single atomic operation.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    ///     Gets a repository for entities of type <see cref="TEntity" /> with identifiers of type <see cref="TId" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
    /// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
    /// <returns>A repository for the specified entity type.</returns>
    IReadWriteRepositoryAsync<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : class, IHasId<TId>;

    /// <summary>
    ///     Gets a custom repository of type <see cref="TRepository" /> for entities of type <see cref="TEntity" /> with
    ///     identifiers of type <see cref="TId" />.
    /// </summary>
    /// <typeparam name="TRepository">The type of the custom repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
    /// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
    /// <returns>A custom repository for the specified entity type.</returns>
    TRepository Repository<TRepository, TEntity, TId>()
        where TRepository : IReadWriteRepositoryAsync<TEntity, TId> where TEntity : class, IHasId<TId>;

    /// <summary>
    ///     Commits the unit of work asynchronously, saving all changes to the repositories.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rolls back the unit of work asynchronously, discarding all changes to the repositories.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}