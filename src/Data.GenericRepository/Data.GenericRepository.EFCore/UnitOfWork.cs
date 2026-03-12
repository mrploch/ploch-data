using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository.Exceptions;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore;

/// <inheritdoc cref="IUnitOfWork" />
/// <summary>
///     Encapsulates a unit of work pattern implementation that manages repository creation and transactions within the
///     specified <see cref="DbContext" /> context.
/// </summary>
/// <typeparam name="TDbContext">
///     The type of the <see cref="DbContext" /> used for this
///     unit of work.
/// </typeparam>
public class UnitOfWork<TDbContext> : IUnitOfWork, IAsyncDisposable where TDbContext : DbContext
{
    private readonly AsyncServiceScope _asyncServiceScope;
    private readonly TDbContext _dbContext;
    private readonly ConcurrentDictionary<string, object> _repositories = [];
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnitOfWork{TDbContext}" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for getting services.</param>
    public UnitOfWork(IServiceProvider serviceProvider)
    {
        _asyncServiceScope = serviceProvider.CreateAsyncScope();
        _dbContext = _asyncServiceScope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    /// <inheritdoc />
    public TRepository Repository<TRepository, TEntity, TId>() where TRepository : IReadWriteRepositoryAsync<TEntity, TId> where TEntity : class, IHasId<TId>
    {
        var cacheKey = $"{typeof(TRepository).FullName}_{typeof(TEntity).FullName}_{typeof(TId).FullName}";

        return (TRepository)_repositories.GetOrAdd(cacheKey, _ => _asyncServiceScope.ServiceProvider.GetRequiredService<TRepository>());
    }

    /// <inheritdoc />
    public IReadWriteRepositoryAsync<TEntity, TId> Repository<TEntity, TId>() where TEntity : class, IHasId<TId>
    {
        var cacheKey = $"{typeof(IReadWriteRepositoryAsync<TEntity, TId>).FullName}";

        return (IReadWriteRepositoryAsync<TEntity, TId>)_repositories.GetOrAdd(cacheKey,
                                                                               _ =>
                                                                                   _asyncServiceScope.ServiceProvider
                                                                                                     .GetRequiredService<
                                                                                                         IReadWriteRepositoryAsync<TEntity, TId>>());
    }

    /// <inheritdoc />
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new DataUpdateConcurrencyException("A concurrency violation is encountered while saving to the database.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new DataUpdateException("Failed to save changes to the underlying data context.", ex);
        }
    }

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _dbContext.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Releases all resources used by the current instance of the <see cref="UnitOfWork{TDbContext}" /> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Disposes the resources used by the unit of work asynchronously.
    /// </summary>
    /// <returns>A <see cref="ValueTask" /> that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the unit of work and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
    ///     When called from the <see cref="Dispose()" /> method, this should be <c>true</c>.
    ///     When called from the finalizer, this should be <c>false</c>.
    /// </param>
    /// <remarks>
    ///     This method is called by the public <see cref="Dispose()" /> method and the <see cref="DisposeAsync()" /> method.
    ///     When called with <paramref name="disposing" /> set to <c>true</c>, it disposes the <see cref="_dbContext" /> and
    ///     <see cref="_asyncServiceScope" /> and marks the instance as disposed.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _dbContext.Dispose();
            _asyncServiceScope.Dispose();

            _disposed = true;
        }
    }

    /// <summary>
    ///     Releases the managed resources used by the unit of work asynchronously.
    ///     This is the core implementation of the asynchronous dispose pattern.
    /// </summary>
    /// <returns>
    ///     A <see cref="ValueTask" /> that represents the asynchronous dispose operation.
    ///     The task completes when all resources have been released.
    /// </returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            await _dbContext.DisposeAsync().ConfigureAwait(false);
            await _asyncServiceScope.DisposeAsync().ConfigureAwait(false);
        }

        _disposed = true;
    }
}
