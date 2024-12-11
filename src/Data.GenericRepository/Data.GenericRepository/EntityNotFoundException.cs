using System;

namespace Ploch.Data.GenericRepository;

public class EntityNotFoundException : InvalidOperationException
{
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
