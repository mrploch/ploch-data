using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a repository that allows reading entities of type <see cref="TEntity" /> from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="IReadRepository{TEntity}" />
public class ReadRepository<TEntity> : QueryableRepository<TEntity>, IReadRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadRepository{TEntity}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
    public ReadRepository(DbContext dbContext) : base(dbContext)
    { }

    public TEntity? GetById(object[] keyValues)
    {
        return DbSet.Find(keyValues);
    }

    public IList<TEntity> GetAll()
    {
        return Entities.ToList();
    }

    public IList<TEntity> GetPage(int pageNumber, int pageSize)
    {
        return GetPageQuery(pageNumber, pageSize).ToList();
    }

    public int Count()
    {
        return Entities.Count();
    }
}

/// <summary>
///     Provides a repository that allows reading entities of type <see cref="TEntity" /> with a specified identifier type
///     from a <see cref="DbContext" />.
/// </summary>
/// <inheritdoc cref="ReadRepository{TEntity}" />
public class ReadRepository<TEntity, TId> : ReadRepository<TEntity>, IReadRepository<TEntity, TId>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ReadRepository{TEntity, TId}" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for reading entities.</param>
    public ReadRepository(DbContext dbContext) : base(dbContext)
    { }

    /// <summary>
    ///     Gets the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to be found.</param>
    /// <returns>The entity found, or null.</returns>
    public TEntity? GetById(TId id)
    {
        return DbSet.Find(id);
    }
}