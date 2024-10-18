using System;
using System.Linq;
using Dawn;
using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows querying entities of type <see cref="TEntity" /> from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc />
public class QueryableRepository<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryableRepository{TEntity}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for querying entities.</param>
    public QueryableRepository(DbContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <inheritdoc />
    public IQueryable<TEntity> Entities => DbSet;

    /// <summary>
    ///     Gets the <see cref="DbContext" /> used for querying entities.
    /// </summary>
    protected DbContext DbContext { get; }

    /// <summary>
    ///     Gets the <see cref="DbSet{TEntity}" /> used for querying entities.
    /// </summary>
    protected DbSet<TEntity> DbSet => DbContext.Set<TEntity>();

    /// <inheritdoc />
    public IQueryable<TEntity> GetPageQuery(int pageNumber, int pageSize, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        Guard.Argument(pageNumber, nameof(pageNumber)).Positive();

        var query = onDbSet == null ? Entities : onDbSet(Entities);

        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize).AsNoTracking();
    }
}