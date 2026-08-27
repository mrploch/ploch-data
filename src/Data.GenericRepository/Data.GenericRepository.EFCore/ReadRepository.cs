using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    /// <remarks>
    ///     This is the one read path that deliberately queries <see cref="QueryableRepository{TEntity}.DbSet" />
    ///     rather than <see cref="QueryableRepository{TEntity}.Entities" />: <c>Find</c> is defined on
    ///     <see cref="DbSet{TEntity}" /> only, not on <see cref="IQueryable{T}" />. Any query shaping applied
    ///     to <see cref="QueryableRepository{TEntity}.Entities" /> therefore does not affect this method.
    /// </remarks>
    public TEntity? GetById(object[] keyValues) => DbSet.Find(keyValues);

    /// <inheritdoc />
    public TEntity? FindFirst(Expression<Func<TEntity, bool>> query, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null) =>
        onDbSet == null ? Entities.FirstOrDefault(query) : onDbSet(Entities).FirstOrDefault(query);

    /// <inheritdoc />
    public IList<TEntity> GetAll(Expression<Func<TEntity, bool>>? query = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        var queryable = onDbSet != null ? onDbSet(Entities) : Entities;

        if (query != null)
        {
            queryable = queryable.Where(query);
        }

        return [.. queryable];
    }

    /// <inheritdoc />
    public IList<TEntity> GetPage(int pageNumber,
                                  int pageSize,
                                  Expression<Func<TEntity, object>>? sortBy = null,
                                  Expression<Func<TEntity, bool>>? query = null,
                                  Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null) =>
        [.. GetPageQuery(pageNumber, pageSize, sortBy, query, onDbSet)];

    /// <inheritdoc />
    public int Count(Expression<Func<TEntity, bool>>? query = null) => query == null ? Entities.Count() : Entities.Count(query);
}

/// <summary>
///     Provides a repository that allows reading entities of type <typeparamref name="TEntity" />
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
    /// <remarks>
    ///     The two paths deliberately differ in what they query. When <paramref name="onDbSet" /> is
    ///     <see langword="null" /> the lookup uses <c>Find</c>, which is defined on
    ///     <see cref="DbSet{TEntity}" /> only and not on <see cref="IQueryable{T}" />, so any query shaping
    ///     applied to <see cref="QueryableRepository{TEntity}.Entities" /> does not affect it. The shaped
    ///     path composes on <see cref="QueryableRepository{TEntity}.Entities" />, like every other
    ///     non-<c>Find</c> read in the hierarchy.
    /// </remarks>
    public TEntity? GetById(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        return onDbSet == null ? DbSet.Find(id) : onDbSet(Entities).FirstOrDefault(e => Equals(e.Id, id));
    }
}
