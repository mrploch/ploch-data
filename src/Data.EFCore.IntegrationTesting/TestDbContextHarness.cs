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
///         harness releases the database context, the initial scope and the root service provider, in
///         that order, and the shared connection last.
///     </para>
///     <para>
///         <strong>The connection is owned only when the harness created it</strong> — that is, when the
///         harness came from
///         <see cref="DbContextServicesRegistrationHelper.BuildHarness{TDbContext}(IServiceCollection,string)" />.
///         When the harness came from the
///         <see cref="DbContextServicesRegistrationHelper.BuildHarness{TDbContext}(IServiceCollection,IDbContextConfigurator)" />
///         overload the connection belongs to the configurator, so the caller must still dispose the
///         configurator itself; the harness deliberately leaves that connection alone.
///     </para>
///     <para>
///         Disposal is resilient: a failure releasing one resource does not prevent the remaining ones
///         from being released. Every failure is surfaced afterwards, aggregated when more than one
///         resource failed.
///     </para>
///     <para>
///         Obtain an instance from
///         <see cref="DbContextServicesRegistrationHelper.BuildHarness{TDbContext}(IServiceCollection,IDbContextConfigurator)" />
///         or its connection-string overload.
///     </para>
///     <example>
///         <code>
///         using var harness = DbContextServicesRegistrationHelper.BuildHarness&lt;MyDbContext&gt;(services);
///         var context = harness.ScopedServiceProvider.GetRequiredService&lt;MyDbContext&gt;();
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
    ///     Asynchronously releases the database context, the initial scope, the root service provider and,
    ///     when the harness created it, the shared connection.
    /// </summary>
    /// <remarks>
    ///     The root provider is released <em>before</em> the shared connection so that singletons whose own
    ///     disposal touches the database still observe an open connection.
    /// </remarks>
    /// <returns>A <see cref="ValueTask" /> that completes when every owned resource has been released.</returns>
    /// <exception cref="AggregateException">Thrown when releasing more than one owned resource failed.</exception>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var failures = new List<Exception>();

        await DisposeSafelyAsync(DbContext, failures).ConfigureAwait(false);
        await DisposeSafelyAsync(_scope, failures).ConfigureAwait(false);
        await DisposeSafelyAsync(RootServiceProvider, failures).ConfigureAwait(false);
        await DisposeSafelyAsync(_connection, failures).ConfigureAwait(false);

        Rethrow(failures);
    }

    /// <summary>
    ///     Releases the database context, the initial scope, the root service provider and, when the harness
    ///     created it, the shared connection.
    /// </summary>
    /// <remarks>
    ///     The root provider is released <em>before</em> the shared connection so that singletons whose own
    ///     disposal touches the database still observe an open connection.
    /// </remarks>
    /// <exception cref="AggregateException">Thrown when releasing more than one owned resource failed.</exception>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var failures = new List<Exception>();

        DisposeSafely(DbContext, failures);
        DisposeSafely(_scope, failures);
        DisposeSafely(RootServiceProvider, failures);
        DisposeSafely(_connection, failures);

        Rethrow(failures);
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

    // CA1031 is suppressed deliberately on the two helpers below: a disposal chain must continue past a
    // failing resource, otherwise one failure leaks every resource queued behind it. Nothing is swallowed —
    // every collected failure is rethrown by Rethrow once the chain has finished, and the IsNonFatal filter
    // keeps process-level failures propagating immediately instead of being aggregated.
#pragma warning disable CA1031
    internal static void DisposeSafely(object? resource, List<Exception> failures)
    {
        try
        {
            (resource as IDisposable)?.Dispose();
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            failures.Add(exception);
        }
    }

    internal static async ValueTask DisposeSafelyAsync(object? resource, List<Exception> failures)
    {
        try
        {
            switch (resource)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);

                    break;
                case IDisposable disposable:
                    disposable.Dispose();

                    break;
                default:
                    // Null, or a resource that owns nothing to release.
                    break;
            }
        }
        catch (Exception exception) when (IsNonFatal(exception))
        {
            failures.Add(exception);
        }
    }
#pragma warning restore CA1031

    /// <summary>
    ///     Determines whether an exception is safe to collect and continue past, rather than one that must
    ///     abandon the disposal chain immediately.
    /// </summary>
    /// <remarks>
    ///     A failing <see cref="IDisposable" /> is an ordinary, recoverable event during test teardown and is
    ///     worth aggregating. A process-level failure is not: continuing to release resources after one is
    ///     pointless, and deferring it behind an <see cref="AggregateException" /> only obscures it.
    /// </remarks>
    /// <param name="exception">The exception raised while releasing a resource.</param>
    /// <returns>
    ///     <see langword="true" /> when the exception may be collected; <see langword="false" /> when it must
    ///     propagate immediately.
    /// </returns>
    private static bool IsNonFatal(Exception exception) =>
        exception is not OutOfMemoryException
            and not StackOverflowException
            and not AccessViolationException
            and not AppDomainUnloadedException
            and not BadImageFormatException
            and not CannotUnloadAppDomainException
            and not InvalidProgramException;

    private static void Rethrow(List<Exception> failures)
    {
        switch (failures.Count)
        {
            case 0:
                return;
            case 1:
                throw failures[0];
            default:
                throw new AggregateException("Releasing the test database harness failed for more than one resource.", failures);
        }
    }
}
