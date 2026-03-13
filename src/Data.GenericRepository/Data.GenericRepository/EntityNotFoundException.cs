using System;
using Ploch.Common.ArgumentChecking;

namespace Ploch.Data.GenericRepository;

/// <summary>
///     Represents an exception that is thrown when a requested entity is not found in the repository.
/// </summary>
/// <remarks>
///     This exception is typically used to indicate that an entity of a specific type and identifier
///     could not be located in the datasource. It extends <see cref="Exception" /> to
///     provide additional metadata about the missing entity, such as its type and identifier.
/// </remarks>
/// <remarks>
///     Initializes a new instance of the <see cref="EntityNotFoundException" /> class with a specified error message,
///     entity type, entity identifier, and a reference to the inner exception that is the cause of this exception.
/// </remarks>
/// <param name="entityType">The type of entity that could not be found.</param>
/// <param name="id">The identifier of the entity that was used in the lookup process.</param>
/// <param name="message">The error message that explains the reason for the exception.</param>
/// <param name="innerException">
///     The exception that is the cause of the current exception or a null reference if no inner exception is specified.
/// </param>
public class EntityNotFoundException(Type entityType, object id, string? message, Exception? innerException) : Exception(message, innerException)
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
    /// <param name="entityType">The entity type.</param>
    /// <param name="id">The entity identifier.</param>
    /// <param name="message">The exception message.</param>
    public EntityNotFoundException(Type entityType, object id, string? message) : this(entityType, id, message, null)
    {
    }

    /// <summary>
    ///     Gets the identifier of the entity that could not be found.
    /// </summary>
    /// <remarks>
    ///     This property represents the unique identifier associated with the entity that was used during
    ///     the lookup process. It is useful for providing additional context about which entity caused
    ///     the exception to be thrown.
    /// </remarks>
    public object Id { get; } = id;

    /// <summary>
    ///     Gets the type of the entity that could not be found.
    /// </summary>
    /// <remarks>
    ///     This property represents the specific type of the entity associated with the exception.
    ///     It is useful for identifying the entity's type when providing diagnostic information
    ///     or handling the exception programmatically.
    /// </remarks>
    public Type EntityType { get; } = entityType;

    /// <summary>
    ///     Creates a new instance of <see cref="EntityNotFoundException" /> for the specified entity type and identifier.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity that could not be found.</typeparam>
    /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
    /// <param name="id">The identifier of the entity that was used in the lookup process.</param>
    /// <returns>
    ///     A new instance of <see cref="EntityNotFoundException" /> with a default error message
    ///     that includes the entity type name and identifier.
    /// </returns>
    /// <remarks>
    ///     This method provides a convenient way to create an exception with a standardized error message
    ///     format for entity not found scenarios.
    /// </remarks>
    public static EntityNotFoundException Create<TEntity, TId>(TId id) =>
        new(typeof(TEntity), id.NotNullOrDefault()!, $"Entity of type {typeof(TEntity).Name} with id {id} not found.");
}
