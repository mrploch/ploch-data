using System.Collections.Generic;
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

    public async Task<TEntity?> GetByIdAsync(object[] keyValues, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(keyValues, cancellationToken);
    }

    public async Task<IList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Entities.ToListAsync(cancellationToken);
    }

    public async Task<IList<TEntity>> GetPageAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await GetPageQuery(pageNumber, pageSize).ToListAsync(cancellationToken);
    }

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

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object?[] { id }, cancellationToken);
    }
}