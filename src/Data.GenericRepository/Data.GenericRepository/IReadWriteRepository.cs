using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides both read and write operations for <see cref="TEntity" /> with a specified
///     identifier type.
/// </summary>
/// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
/// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
public interface IReadWriteRepository<TEntity, in TId> : IReadRepository<TEntity, TId>, IWriteRepository<TEntity, TId>
    where TEntity : class, IHasId<TId>
{ }