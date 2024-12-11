using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.Model;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member - false/positive, docs already inherited

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows asynchronous reading and writing of entities of type
///     <typeparamref name="TEntity" /> with a specified identifier type from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="IReadWriteRepositoryAsync{TEntity,TId}" />
/// <remarks>
///     Initializes a new instance of the <see cref="ReadWriteRepositoryAsync{TEntity, TId}" /> class.
/// </remarks>
/// <param name="dbContext">The <see cref="DbContext" /> to use for reading and writing entities.</param>
public class ReadWriteRepositoryAsync<TEntity, TId>(DbContext dbContext, IAuditEntityHandler auditEntityHandler)
    : ReadRepositoryAsync<TEntity, TId>(dbContext, auditEntityHandler), IReadWriteRepositoryAsync<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        auditEntityHandler.HandleCreation(entity);
        await DbSet.AddAsync(entity, cancellationToken);

        return entity;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Guard does not enumerate items.")]
    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entities, nameof(entities)).NotNull();

        foreach (var entity in entities)
        {
            auditEntityHandler.HandleCreation(entity);
        }

        await DbSet.AddRangeAsync(entities, cancellationToken);

        return entities;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        DbSet.Remove(entity);

        return Task.CompletedTask;
    }

    public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken: cancellationToken);

        if (entity == null)
        {
            throw EntityNotFoundException.Create<TEntity, TId>(id);
        }

        await DeleteAsync(entity, cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        var exist = await GetByIdAsync(entity.Id, cancellationToken: cancellationToken);
        if (exist == null)
        {
            throw EntityNotFoundException.Create<TEntity, TId>(entity.Id);
        }

        auditEntityHandler.HandleModification(entity);
        DbContext.Entry(exist).CurrentValues.SetValues(entity);
    }
}
