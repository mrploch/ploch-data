using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.GenericRepository.EFCore.Specification;

public static class ReadRepositoryAsyncExtensions
{
    /// <summary>
    ///     Asynchronously gets a collection of entities that match the specified criteria.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="repository">The repository to query.</param>
    /// <param name="specification">The specification that defines the criteria for the query.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a collection of entities that
    ///     match the specified criteria.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<IEnumerable<TEntity>> GetAllBySpecificationAsync<TEntity>(this IQueryableRepository<TEntity> repository,
                                                                                       ISpecification<TEntity> specification,
                                                                                       CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var query = SpecificationEvaluator.Default.GetQuery(repository.Entities, specification);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Asynchronously gets a single entity that matches the specified criteria.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="repository">The repository to query.</param>
    /// <param name="specification">The specification that defines the criteria for the query.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the entity that matches
    ///     the specified criteria, or <c>null</c> if no entity matches.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static async Task<TEntity?> GetBySpecificationAsync<TEntity>(this IQueryableRepository<TEntity> repository,
                                                                        ISingleResultSpecification<TEntity> specification,
                                                                        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var query = SpecificationEvaluator.Default.GetQuery(repository.Entities, specification);

        return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }
}
