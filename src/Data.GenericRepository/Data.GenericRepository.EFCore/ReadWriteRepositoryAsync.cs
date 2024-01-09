using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.EntityFrameworkCore;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows asynchronous reading and writing of entities of type <see cref="TEntity" /> with
///     a specified identifier type from a <see cref="DbContext" />.
/// </summary>
/// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
/// <inheritdoc cref="ReadRepositoryAsync{TEntity,TId}" />
/// <inheritdoc cref="IReadRepositoryAsync{TEntity, TId}" />
public class ReadWriteRepositoryAsync<TEntity, TId> : ReadRepositoryAsync<TEntity, TId>, IReadWriteRepositoryAsync<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadWriteRepositoryAsync{TEntity, TId}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading and writing entities.</param>
    public ReadWriteRepositoryAsync(DbContext dbContext) : base(dbContext)
    { }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);

        return entity;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration", Justification = "Guard does not enumerate items.")]
    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entities, nameof(entities)).NotNull();

        await DbContext.AddRangeAsync(entities, cancellationToken);

        return entities;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        DbContext.Set<TEntity>().Remove(entity);

        return Task.CompletedTask;
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Guard.Argument(entity, nameof(entity)).NotNull();

        var exist = await GetByIdAsync(entity.Id, cancellationToken);
        if (exist == null)
        {
            throw new InvalidOperationException($"Entity with id {entity.Id} not found");
        }

        DbContext.Entry(exist).CurrentValues.SetValues(entity);
    }
}