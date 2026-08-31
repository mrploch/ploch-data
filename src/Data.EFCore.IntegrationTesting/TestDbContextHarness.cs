using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.IntegrationTesting;

/// <summary>
///     Owns every disposable resource created when an integration-test database context is built:
///     the root service provider, the initial service scope, the shared database connection (when the
///     harness created one) and the initial <typeparamref name="TDbContext" /> instance.
/// </summary>
/// <remarks>
///     <para>
///         The harness exists to give callers a single, unambiguous ownership contract. Disposing the
///         harness releases all four resources in the correct order — context, scope, connection, root
///         provider — so a test never has to remember which of them it is responsible for.
///     </para>
///     <para>
///         Obtain an instance from
///         <see cref="DbContextServicesRegistrationHelper.BuildHarness{TDbContext}(IServiceCollection,IDbContextConfigurator)" />
///         or its connection-string overload.
///     </para>
///     <example>
///         <code>
///         using var harness = DbContextServicesRegistrationHelper.BuildHarness&lt;MyDbContext&gt;(services);
///         var repository = harness.ScopedServiceProvider.GetRequiredService&lt;IMyRepository&gt;();
///         </code>
///     </example>
/// </remarks>
/// <typeparam name="TDbContext">The type of the database context owned by the harness.</typeparam>
public sealed class TestDbContextHarness<TDbContext> : IDisposable, IAsyncDisposable where TDbContext : DbContext
{
    private readonly SqliteConnection? _connection;
    private readonly IServiceScope _scope;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestDbContextHarness{TDbContext}" /> class.
    /// </summary>
    /// <param name="rootServiceProvider">The root service provider that owns every registered service.</param>
    /// <param name="scope">The initial service scope from which <paramref name="dbContext" /> was resolved.</param>
    /// <param name="dbContext">The prepared database context.</param>
    /// <param name="connection">
    ///     The shared database connection created by the builder, or <see langword="null" /> when the connection is owned by
    ///     the configurator rather than by the harness.
    /// </param>
    internal TestDbContextHarness(IServiceProvider rootServiceProvider, IServiceScope scope, TDbContext dbContext, SqliteConnection? connection)
    {
        RootServiceProvider = rootServiceProvider;
        _scope = scope;
        DbContext = dbContext;
        _connection = connection;
    }

    /// <summary>
    ///     Gets the root (non-scoped) service provider.
    /// </summary>
    /// <remarks>
    ///     Use this to create additional scopes. Resolving scoped services directly from the root provider
    ///     defeats scoped lifetimes and is almost never what a test wants.
    /// </remarks>
    public IServiceProvider RootServiceProvider { get; }

    /// <summary>
    ///     Gets the service provider of the initial scope, from which <see cref="DbContext" /> was resolved.
    /// </summary>
    public IServiceProvider ScopedServiceProvider => _scope.ServiceProvider;

    /// <summary>
    ///     Gets the prepared database context resolved from <see cref="ScopedServiceProvider" />.
    /// </summary>
    public TDbContext DbContext { get; }

    /// <summary>
    ///     Asynchronously releases the root provider, the initial scope, the shared connection and the database context.
    /// </summary>
    /// <returns>A <see cref="ValueTask" /> that completes when every owned resource has been released.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        await DbContext.DisposeAsync().ConfigureAwait(false);

        if (_scope is IAsyncDisposable asyncScope)
        {
            await asyncScope.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _scope.Dispose();
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        if (RootServiceProvider is IAsyncDisposable asyncRoot)
        {
            await asyncRoot.DisposeAsync().ConfigureAwait(false);
        }
        else if (RootServiceProvider is IDisposable disposableRoot)
        {
            disposableRoot.Dispose();
        }
    }

    /// <summary>
    ///     Releases the root provider, the initial scope, the shared connection and the database context.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        DbContext.Dispose();
        _scope.Dispose();
        _connection?.Dispose();

        if (RootServiceProvider is IDisposable disposableRoot)
        {
            disposableRoot.Dispose();
        }
    }

    /// <summary>
    ///     Deconstructs the harness into the triple returned by the legacy
    ///     <see cref="DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,IDbContextConfigurator)" />
    ///     methods.
    /// </summary>
    /// <param name="rootProvider">Receives <see cref="RootServiceProvider" />.</param>
    /// <param name="scopedProvider">Receives <see cref="ScopedServiceProvider" />.</param>
    /// <param name="dbContext">Receives <see cref="DbContext" />.</param>
    public void Deconstruct(out IServiceProvider rootProvider, out IServiceProvider scopedProvider, out TDbContext dbContext)
    {
        rootProvider = RootServiceProvider;
        scopedProvider = ScopedServiceProvider;
        dbContext = DbContext;
    }
}
