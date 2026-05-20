using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ploch.Common.ArgumentChecking;

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
public class QueryableRepository<TEntity>(DbContext dbContext) : IQueryableRepository<TEntity> where TEntity : class
{
    /// <inheritdoc />
    public IQueryable<TEntity> Entities => DbSet;

    /// <summary>
    ///     Gets the <see cref="DbContext" /> used for querying entities.
    /// </summary>
    protected DbContext DbContext { get; } = dbContext.NotNull();

    /// <summary>
    ///     Gets the <see cref="DbSet{TEntity}" /> used for querying entities.
    /// </summary>
    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    /// <inheritdoc />
    public IQueryable<TEntity> GetPageQuery(int pageNumber,
                                            int pageSize,
                                            Expression<Func<TEntity, object>>? sortBy = null,
                                            Expression<Func<TEntity, bool>>? query = null,
                                            Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        pageNumber.Positive();
        pageSize.Positive();

        var dbSetQuery = onDbSet != null ? onDbSet(Entities) : Entities;

        if (query != null)
        {
            dbSetQuery = dbSetQuery.Where(query);
        }

        if (sortBy != null)
        {
            dbSetQuery = dbSetQuery.OrderBy(sortBy);
        }

        return dbSetQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking();
    }
}
