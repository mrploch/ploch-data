using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository;

/// <summary>
///     Defines a repository that provides read operations for a collection of a <typeparamref name="TEntity" />.
/// </summary>
/// <inheritdoc />
public interface IReadRepository<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Gets the entity with the specified primary key values.
    /// </summary>
    /// <param name="keyValues">The values of the primary key for the entity to be found.</param>
    /// <returns>The entity found, or null.</returns>
    TEntity? GetById(object[] keyValues);

    /// <summary>
    ///     Finds the first entity that matches the specified query.
    /// </summary>
    /// <param name="query">A LINQ expression used to filter the entities.</param>
    /// <param name="onDbSet">
    ///     An optional function used to <em>shape</em> the query — for example eager loading with
    ///     <c>Include</c> / <c>ThenInclude</c>, ordering, or <c>AsNoTracking</c>. Do not filter here; express
    ///     filtering with <paramref name="query" /> instead.
    /// </param>
    /// <returns>The first entity that matches the query, or null if none found.</returns>
    /// <remarks>
    ///     This method executes synchronously and therefore takes no <see cref="System.Threading.CancellationToken" />.
    ///     Use <see cref="IReadRepositoryAsync{TEntity}.FindFirstAsync" /> when cancellation is required.
    /// </remarks>
    TEntity? FindFirst(Expression<Func<TEntity, bool>> query, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);

    /// <summary>
    ///     Gets all entities from the repository, optionally filtered by a query.
    /// </summary>
    /// <param name="query">A LINQ expression used to filter the entities. When null, all entities are returned.</param>
    /// <param name="onDbSet">
    ///     An optional function used to <em>shape</em> the query — for example eager loading with
    ///     <c>Include</c> / <c>ThenInclude</c>, ordering, or <c>AsNoTracking</c>. Do not filter here; express
    ///     filtering with <paramref name="query" /> instead.
    /// </param>
    /// <returns>A list of the entities matching <paramref name="query" />, or all entities when it is null.</returns>
    /// <example>
    /// <code>
    /// // Filter with query, eager-load with onDbSet.
    /// var posts = repository.GetAll(post => post.IsPublished, q => q.Include(post => post.Tags));
    /// </code>
    /// </example>
    IList<TEntity> GetAll(Expression<Func<TEntity, bool>>? query = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);

    /// <summary>
    ///     Gets a page of entities from the repository.
    /// </summary>
    /// <param name="pageNumber">The number of the page to get starting from 1.</param>
    /// <param name="pageSize">The size of the page to get.</param>
    /// <param name="sortBy">An optional sort-by expression applied after filtering.</param>
    /// <param name="query">A LINQ expression used to filter the entities.</param>
    /// <param name="onDbSet">
    ///     An optional function used to <em>shape</em> the query — for example eager loading with
    ///     <c>Include</c> / <c>ThenInclude</c>, ordering, or <c>AsNoTracking</c>. Do not filter here; express
    ///     filtering with <paramref name="query" /> instead.
    /// </param>
    /// <returns>A list of entities for the specified page.</returns>
    /// <remarks>
    ///     The parameter order matches <c>IReadRepositoryAsync&lt;TEntity&gt;.GetPageAsync</c>. Prefer named
    ///     arguments — <paramref name="sortBy" /> accepts any lambda whose body converts to
    ///     <see cref="object" />, so a positional predicate binds to it silently.
    /// </remarks>
    /// <example>
    ///     <paramref name="sortBy" /> was added at this position in v4.0. A three-argument positional
    ///     call written against the previous signature still compiles and changes meaning, because a
    ///     <see cref="bool" />-bodied lambda boxes to <see cref="object" />:
    ///     <code>
    /// // Before v4.0: filtered to published posts.
    /// // Since v4.0:  orders by a boxed bool and returns EVERY row, unfiltered.
    /// repository.GetPage(1, 20, post => post.IsPublished);
    ///
    /// // Name the argument to get the intended behaviour:
    /// repository.GetPage(1, 20, query: post => post.IsPublished);
    ///     </code>
    /// </example>
    IList<TEntity> GetPage(int pageNumber,
                           int pageSize,
                           Expression<Func<TEntity, object>>? sortBy = null,
                           Expression<Func<TEntity, bool>>? query = null,
                           Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);

    /// <summary>
    ///     Gets the count of entities in the repository, optionally filtered by a query.
    /// </summary>
    /// <param name="query">A LINQ expression used to filter the entities. When null, all entities are counted.</param>
    /// <returns>The count of entities matching <paramref name="query" />, or of all entities when it is null.</returns>
    int Count(Expression<Func<TEntity, bool>>? query = null);
}

/// <summary>
///     Defines a repository that provides read operations for a collection of <typeparamref name="TEntity" /> with a
///     specified
///     identifier type.
/// </summary>
/// <inheritdoc />
public interface IReadRepository<TEntity, in TId> : IReadRepository<TEntity>
    where TEntity : class, IHasId<TId>
{
    /// <summary>
    ///     Gets the entity with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to be found.</param>
    /// <param name="onDbSet">
    ///     An optional function used to <em>shape</em> the query — for example eager loading with
    ///     <c>Include</c> / <c>ThenInclude</c>, ordering, or <c>AsNoTracking</c>. Do not filter here: this method
    ///     looks an entity up by its identifier, and a filter applied through this parameter would silently turn a
    ///     found entity into <see langword="null" />. Use <see cref="IReadRepository{TEntity}.FindFirst" /> or
    ///     <see cref="IReadRepository{TEntity}.GetAll" /> when an additional predicate is required.
    /// </param>
    /// <returns>The entity found, or null.</returns>
    /// <remarks>
    ///     Supplying <paramref name="onDbSet" /> also changes how the entity is retrieved: without it the entity may
    ///     be served from the change tracker without querying the database, whereas with it a database query is
    ///     always executed.
    /// </remarks>
    TEntity? GetById(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);
}
