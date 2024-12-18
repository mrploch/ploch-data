using System;
using System.Linq;
using System.Linq.Expressions;
using Dawn;
using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows querying entities of type <typeparamref name="TEntity" /> from a
///     <see cref="DbContext" />.
/// </summary>
/// <inheritdoc />
/// <remarks>
///     Initializes a new instance of the <see cref="QueryableRepository{TEntity}" /> class.
/// </remarks>
/// <param name="dbContext">The <see cref="DbContext" /> to use for querying entities.</param>
public class QueryableRepository<TEntity>(DbContext dbContext) : IQueryableRepository<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public IQueryable<TEntity> Entities => DbSet;

    /// <summary>
    ///     Gets the <see cref="DbContext" /> used for querying entities.
    /// </summary>
    protected DbContext DbContext { get; } = dbContext;

    /// <summary>
    ///     Gets the <see cref="DbSet{TEntity}" /> used for querying entities.
    /// </summary>
    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    /// <inheritdoc />
    public IQueryable<TEntity> GetPageQuery(int pageNumber,
                                            int pageSize,
                                            Func<TEntity, object>? sortBy = null,
                                            Expression<Func<TEntity, bool>>? query = null,
                                            Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        Guard.Argument(pageNumber, nameof(pageNumber)).Positive();
        var orderedEnumerable = sortBy != null ? Entities.OrderBy(sortBy).AsQueryable() : Entities;

        var dbSetQuery = onDbSet == null ? orderedEnumerable : onDbSet(orderedEnumerable);

        dbSetQuery = query == null ? dbSetQuery : dbSetQuery.Where(query);

        return dbSetQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking();
    }
}
