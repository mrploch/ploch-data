using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows reading entities of type <typeparamref name="TEntity" /> from a
///     <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="IReadRepository{TEntity}" />
/// <remarks>
///     Initializes a new instance of the <see cref="ReadRepository{TEntity}" /> class.
/// </remarks>
/// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
public class ReadRepository<TEntity>(DbContext dbContext) : QueryableRepository<TEntity>(dbContext), IReadRepository<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public TEntity? GetById(object[] keyValues) => DbSet.Find(keyValues);

    /// <inheritdoc />
    public TEntity? FindFirst(Expression<Func<TEntity, bool>> query,
                              Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                              CancellationToken cancellationToken = default) => onDbSet == null ? DbSet.FirstOrDefault(query) : onDbSet(DbSet).FirstOrDefault(query);

    /// <inheritdoc />
    public IList<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null) => onDbSet == null ? [.. Entities] : onDbSet(Entities).ToList();

    /// <inheritdoc />
    public IList<TEntity> GetPage(int pageNumber,
                                  int pageSize,
                                  Expression<Func<TEntity, bool>>? query = null,
                                  Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null) =>
        [.. GetPageQuery(pageNumber, pageSize, query: query, onDbSet: onDbSet)];

    /// <inheritdoc />
    public int Count() => Entities.Count();
}

/// <summary>
///     Provides a! repository that allows reading entities of type <typeparamref name="TEntity" />
///     with a specified identifier type from a <see cref="DbContext" />.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
/// <inheritdoc cref="ReadRepository{TEntity}" />
/// <remarks>
///     Initializes a new instance of the <see cref="ReadRepository{TEntity, TId}" /> class.
/// </remarks>
/// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
public class ReadRepository<TEntity, TId>(DbContext dbContext) : ReadRepository<TEntity>(dbContext), IReadRepository<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    /// <inheritdoc />
    public TEntity? GetById(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        return onDbSet == null ? DbSet.Find(id) : onDbSet(DbSet).FirstOrDefault(e => Equals(e.Id, id));
    }
}
