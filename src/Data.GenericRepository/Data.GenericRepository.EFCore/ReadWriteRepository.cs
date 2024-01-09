using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using Microsoft.EntityFrameworkCore;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows reading and writing entities of type <see cref="TEntity" /> with a specified
///     identifier type from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="ReadRepository{TEntity, TId}" />
/// <inheritdoc cref="IReadWriteRepository{TEntity,TId}" />
public class ReadWriteRepository<TEntity, TId> : ReadRepository<TEntity, TId>, IReadWriteRepository<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadWriteRepository{TEntity, TId}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading and writing entities.</param>
    public ReadWriteRepository(DbContext dbContext) : base(dbContext)
    { }

    public TEntity Add(TEntity entity)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        DbContext.Set<TEntity>().Add(entity);

        return entity;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Guard does not enumerate items.")]
    public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
    {
        Guard.Argument(entities, nameof(entities)).NotNull();

        DbContext.AddRange(entities);

        return entities;
    }

    public void Delete(TEntity entity)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        DbContext.Set<TEntity>().Remove(entity);
    }

    public void Update(TEntity entity)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        var exist = GetById(entity.Id);
        if (exist == null)
        {
            throw new InvalidOperationException($"Entity with id {entity.Id} not found");
        }

        DbContext.Entry(exist).CurrentValues.SetValues(entity);
    }
}