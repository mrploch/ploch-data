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
///     Provides a repository that allows asynchronous reading of entities of type <see cref="TEntity" /> from a
///     <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="IReadRepositoryAsync{TEntity}" />
public class ReadRepositoryAsync<TEntity> : QueryableRepository<TEntity>, IReadRepositoryAsync<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadRepositoryAsync{TEntity}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public ReadRepositoryAsync(DbContext dbContext) : base(dbContext)
    { }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(keyValues, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? query = null,
                                                  Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                                                  CancellationToken cancellationToken = default)
    {
        return onDbSet == null ? await Entities.ToListAsync(cancellationToken) : await onDbSet(Entities).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IList<TEntity>> GetPageAsync(int pageNumber,
                                                   int pageSize,
                                                   Expression<Func<TEntity, bool>>? query = null,
                                                   Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                                                   CancellationToken cancellationToken = default)
    {
        return await GetPageQuery(pageNumber, pageSize, query, onDbSet).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return Entities.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> query,
                                               Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                                               CancellationToken cancellationToken = default)
    {
        var entities = onDbSet != null ? onDbSet(Entities) : Entities;

        return await entities.FirstOrDefaultAsync(query, cancellationToken);
    }
}

/// <summary>
///     Provides a repository that allows asynchronous reading of entities of type <see cref="TEntity" /> with a specified
///     identifier type from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="IReadRepositoryAsync{TEntity, TId}" />
public class ReadRepositoryAsync<TEntity, TId> : ReadRepositoryAsync<TEntity>, IReadRepositoryAsync<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadRepositoryAsync{TEntity, TId}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
    // ReSharper disable once MemberCanBeProtected.Global
    public ReadRepositoryAsync(DbContext dbContext) : base(dbContext)
    { }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null, CancellationToken cancellationToken = default)
    {
        return onDbSet == null ? await DbSet.FindAsync([id], cancellationToken) : await onDbSet(DbSet).FirstOrDefaultAsync(e => Equals(e.Id, id), cancellationToken);
    }
}