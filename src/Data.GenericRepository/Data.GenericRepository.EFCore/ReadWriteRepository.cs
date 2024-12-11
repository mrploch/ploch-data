using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows reading and writing entities of type <typeparamref name="TEntity" /> with a
///     specified identifier type from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="ReadRepository{TEntity, TId}" />
/// <inheritdoc cref="IReadWriteRepository{TEntity,TId}" />
/// <remarks>
///     Initializes a new instance of the <see cref="ReadWriteRepository{TEntity, TId}" /> class.
/// </remarks>
/// <param name="dbContext">The <see cref="DbContext" /> to use for reading and writing entities.</param>
public class ReadWriteRepository<TEntity, TId>(DbContext dbContext, IAuditEntityHandler auditEntityHandler)
    : ReadRepository<TEntity, TId>(dbContext, auditEntityHandler), IReadWriteRepository<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    /// <inheritdoc />
    public TEntity Add(TEntity entity)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        auditEntityHandler.HandleCreation(entity);
        DbContext.Set<TEntity>().Add(entity);

        return entity;
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Guard does not enumerate items.")]
    public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
    {
        Guard.Argument(entities, nameof(entities)).NotNull();

        foreach (var entity in entities)
        {
            auditEntityHandler.HandleCreation(entity);
        }

        DbContext.AddRange(entities);

        return entities;
    }

    /// <inheritdoc />
    public void Delete(TEntity entity)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        DbContext.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public void Delete(TId id)
    {
        var entity = GetById(id);

        if (entity == null)
        {
            throw EntityNotFoundException.Create<TEntity, TId>(id);
        }
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        var exist = GetById(entity.Id);
        if (exist == null)
        {
            throw EntityNotFoundException.Create<TEntity, TId>(entity.Id);
        }

        auditEntityHandler.HandleModification(entity);

        DbContext.Entry(exist).CurrentValues.SetValues(entity);
    }
}
