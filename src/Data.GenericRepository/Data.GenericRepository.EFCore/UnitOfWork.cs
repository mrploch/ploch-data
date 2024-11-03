using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore;

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

    /// <inheritdoc />
    public TRepository Repository<TRepository, TEntity, TId>()
        where TRepository : IReadWriteRepositoryAsync<TEntity, TId> where TEntity : class, IHasId<TId>
    {
        var type = typeof(TEntity).Name;

        return (TRepository)_repositories.GetOrAdd(type, _ => _serviceProvider.GetRequiredService<TRepository>());
    }

    /// <inheritdoc />
    public IReadWriteRepositoryAsync<TEntity, TId> Repository<TEntity, TId>()
        where TEntity : class, IHasId<TId>
    {
        var type = typeof(TEntity).Name;

        return (IReadWriteRepositoryAsync<TEntity, TId>)_repositories.GetOrAdd(type, _ => _serviceProvider.GetRequiredService<IReadWriteRepositoryAsync<TEntity, TId>>());
    }

    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
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