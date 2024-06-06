﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows asynchronous reading of entities of type <see cref="TEntity" /> from a
///     <see cref="DbContext" />.
/// </summary>
/// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
/// <inheritdoc cref="IReadRepository{TEntity}" />
/// <inheritdoc cref="QueryableRepository{TEntity}" />
public class ReadRepositoryAsync<TEntity> : QueryableRepository<TEntity>, IReadRepositoryAsync<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadRepositoryAsync{TEntity}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
    public ReadRepositoryAsync(DbContext dbContext) : base(dbContext)
    { }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(keyValues, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null, CancellationToken cancellationToken = default)
    {
        return onDbSet == null ? await Entities.ToListAsync(cancellationToken) : await onDbSet(Entities).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IList<TEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await GetPageQuery(pageNumber, pageSize).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return Entities.CountAsync(cancellationToken);
    }
}

/// <summary>
///     Provides a repository that allows asynchronous reading of entities of type <see cref="TEntity" /> with a specified
///     identifier type from a <see cref="DbContext" />.
/// </summary>
/// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
/// <typeparam name="TEntity">The type of entity.</typeparam>
/// <inheritdoc cref="ReadRepositoryAsync{TEntity}" />
/// <inheritdoc cref="IReadRepositoryAsync{TEntity, TId}" />
public class ReadRepositoryAsync<TEntity, TId> : ReadRepositoryAsync<TEntity>, IReadRepositoryAsync<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadRepositoryAsync{TEntity, TId}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
    public ReadRepositoryAsync(DbContext dbContext) : base(dbContext)
    { }

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null, CancellationToken cancellationToken = default)
    {
        return onDbSet == null ? await DbSet.FindAsync([id], cancellationToken) : await onDbSet(DbSet).FirstOrDefaultAsync(e => Equals(e.Id, id), cancellationToken);
    }
}