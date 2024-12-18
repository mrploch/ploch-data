using System;

namespace Ploch.Data.GenericRepository;

/// <summary>
///     Represents an exception that is thrown when a requested entity is not found in the repository.
/// </summary>
/// <remarks>
///     This exception is typically used to indicate that an entity of a specific type and identifier
///     could not be located in the datasource. It extends <see cref="Exception" /> to
///     provide additional metadata about the missing entity, such as its type and identifier.
/// </remarks>
public class EntityNotFoundException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityNotFoundException" /> class.
    ///     Represents an exception that is thrown when a specific entity cannot be located in the repository.
    /// </summary>
    /// <remarks>
    ///     This exception contains information about the type of the entity and the identifier used during the failed lookup
    ///     process.
    ///     It is intended to help identify missing entities in the data layer or repository pattern.
    /// </remarks>
    public EntityNotFoundException(Type entityType, object id, string? message) : this(entityType, id, message, null)
    { }

    public EntityNotFoundException(Type entityType, object id, string? message, Exception? innerException) : base(message, innerException)
    {
        EntityType = entityType;
        Id = id;
    }

    public object Id { get; }

    public Type EntityType { get; }

    public static EntityNotFoundException Create<TEntity, TId>(TId id) => new(typeof(TEntity), id, $"Entity of type {typeof(TEntity).Name} with id {id} not found.");
}
