using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore;

/// <summary>
///     Provides a unit of work that allows managing repositories and transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _dbContext;
    private readonly ConcurrentDictionary<string, object> _repositories = new();
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnitOfWork" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for getting services.</param>
    /// <param name="dbContext">The <see cref="DbContext" /> to use for managing the unit of work.</param>
    public UnitOfWork(IServiceProvider serviceProvider, DbContext dbContext)
    {
        _serviceProvider = serviceProvider;
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    ///     Gets the repository for the specified entity type and identifier type.
    /// </summary>
    /// <typeparam name="TRepository">The type of the repository.</typeparam>
    /// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
    /// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
    /// <returns>The repository for the specified entity type and identifier type.</returns>
    public TRepository Repository<TRepository, TEntity, TId>()
        where TRepository : IReadWriteRepositoryAsync<TEntity, TId> where TEntity : class, IHasId<TId>
    {
        var type = typeof(TEntity).Name;

        return (TRepository)_repositories.GetOrAdd(type, _ => _serviceProvider.GetRequiredService<TRepository>());
    }

    /// <summary>
    ///     Gets the repository for the specified entity type and identifier type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities in the repository.</typeparam>
    /// <typeparam name="TId">The type of the identifier for the entities in the repository.</typeparam>
    /// <returns>The repository for the specified entity type and identifier type.</returns>
    public IReadWriteRepositoryAsync<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        var type = typeof(TEntity).Name;

        return (IReadWriteRepositoryAsync<TEntity, TId>)_repositories.GetOrAdd(type, _ => _serviceProvider.GetRequiredService<IReadWriteRepositoryAsync<TEntity, TId>>());
    }

    /// <summary>
    ///     Asynchronously commits all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Asynchronously rolls back all changes made in this unit of work to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _dbContext.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Releases all resources used by the current instance of the <see cref="UnitOfWork" /> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the <see cref="UnitOfWork" /> and optionally releases the managed
    ///     resources.
    /// </summary>
    /// <param name="disposing">
    ///     true to release both managed and unmanaged resources; false to release only unmanaged
    ///     resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _dbContext.Dispose();
        }

        _disposed = true;
    }
}