using System;
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
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:Ploch.Common.Data.GenericRepository.EFCore.ReadRepository`1" /> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="T:Microsoft.EntityFrameworkCore.DbContext" /> to use for reading entities.</param>
    public ReadRepository(DbContext dbContext) : base(dbContext)
    { }

    /// <inheritdoc />
    public TEntity? GetById(object[] keyValues)
    {
        return DbSet.Find(keyValues);
    }

    /// <inheritdoc />
    public IList<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        return onDbSet == null ? Entities.ToList() : onDbSet(Entities).ToList();
    }

    /// <inheritdoc />
    public IList<TEntity> GetPage(int pageNumber, int pageSize)
    {
        return GetPageQuery(pageNumber, pageSize).ToList();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public TEntity? GetById(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)
    {
        return onDbSet == null ? DbSet.Find(id) : onDbSet(DbSet).FirstOrDefault(e => Equals(e.Id, id));
    }
}