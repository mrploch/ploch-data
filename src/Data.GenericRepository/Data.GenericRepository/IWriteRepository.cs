using System.Collections.Generic;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides write operations for <stypeparamref name="TEntity" /> with a specified
///     identifier type.
/// </summary>
/// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
/// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
public interface IWriteRepository<TEntity, in TId>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Adds the specified entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity.</returns>
    TEntity Add(TEntity entity);

    /// <summary>
    ///     Adds the specified entities to the repository.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <returns>The added entities.</returns>
    IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    ///     Updates the specified entity in the repository.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The update applies every scalar property of <paramref name="entity" /> to the stored entity.
    ///         Partial updates are not supported: any property the caller leaves unset is written to the
    ///         store with its default value. Use the fetch-modify-update pattern to change a subset of
    ///         properties safely.
    ///     </para>
    ///     <para>
    ///         Creation-audit properties are the exception. When auditing is enabled, for entities
    ///         implementing <see cref="IHasCreatedTime" /> or <see cref="IHasCreatedBy" />, the persisted
    ///         <c>CreatedTime</c> and <c>CreatedBy</c> values are preserved and cannot be changed
    ///         through this method — creation audit is write-once, set when the entity is added.
    ///     </para>
    /// </remarks>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    ///     Deletes the specified entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);

    /// <summary>
    ///     Deletes the entity with specified id from the repository.
    /// </summary>
    /// <param name="id">The entity id to delete.</param>
    void Delete(TId id);
}
