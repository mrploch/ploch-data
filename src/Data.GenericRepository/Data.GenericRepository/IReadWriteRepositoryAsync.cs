using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides both asynchronous read and write operations for a collection of
///     <see cref="TEntity" /> with a specified identifier type.
/// </summary>
/// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
/// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
public interface IReadWriteRepositoryAsync<TEntity, in TId> : IReadRepositoryAsync<TEntity, TId>, IWriteRepositoryAsync<TEntity, TId>
    where TEntity : class, IHasId<TId>
{ }