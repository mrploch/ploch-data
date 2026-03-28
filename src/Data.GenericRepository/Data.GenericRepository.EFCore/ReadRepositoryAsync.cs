using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows asynchronous reading of entities of type <typeparamref name="TEntity" /> from a
///     <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="IReadRepositoryAsync{TEntity}" />
public class ReadRepositoryAsync<TEntity>(DbContext dbContext, IAuditEntityHandler auditEntityHandler)
    : QueryableRepository<TEntity>(dbContext), IReadRepositoryAsync<TEntity> where TEntity : class
{
    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync(keyValues, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">When source is null.</exception>
    /// <exception cref="OperationCanceledException">When operation is cancelled.</exception>
    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public async Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? query = null,
                                                  Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                                                  CancellationToken cancellationToken = default)
    {
        var queryable = onDbSet != null ? onDbSet(Entities) : Entities;

        if (query != null)
        {
            queryable = queryable.Where(query);
        }

        var result = await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in result)
        {
            auditEntityHandler.HandleAccess(entity);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IList<TEntity>> GetPageAsync(int pageNumber,
                                                   int pageSize,
                                                   Expression<Func<TEntity, object>>? sortBy = null,
                                                   Expression<Func<TEntity, bool>>? query = null,
                                                   Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                                                   CancellationToken cancellationToken = default) =>
        await GetPageQuery(pageNumber, pageSize, sortBy, query, onDbSet).ToListAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<int> CountAsync(Expression<Func<TEntity, bool>>? query = null, CancellationToken cancellationToken = default) =>
        query == null ? Entities.CountAsync(cancellationToken) : Entities.Where(query).CountAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> query,
                                               Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                                               CancellationToken cancellationToken = default)
    {
        var entities = onDbSet != null ? onDbSet(Entities) : Entities;

        return await entities.FirstOrDefaultAsync(query, cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
///     Provides a repository that allows asynchronous reading of entities of type <typeparamref name="TEntity" /> with a
///     specified
///     identifier type from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="IReadRepositoryAsync{TEntity, TId}" />
public class ReadRepositoryAsync<TEntity, TId>(DbContext dbContext, IAuditEntityHandler auditEntityHandler)
    : ReadRepositoryAsync<TEntity>(dbContext, auditEntityHandler), IReadRepositoryAsync<TEntity, TId> where TEntity : class, IHasId<TId>
{
    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(TId id,
                                             Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                                             CancellationToken cancellationToken = default)
    {
        var result =
            onDbSet == null ?
                await DbSet.FindAsync([id], cancellationToken) :
                await onDbSet(DbSet).FirstOrDefaultAsync(e => Equals(e.Id, id), cancellationToken);

        if (result != null)
        {
            auditEntityHandler.HandleAccess(result);
        }

        return result;
    }
}
