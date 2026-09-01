using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ploch.Data.EFCore.SqLite;

namespace Ploch.Data.EFCore.IntegrationTesting;

/// <summary>
///     Abstract base class for integration tests that involve Entity Framework Core.
///     Provides initialization and configuration of the database context and services.
/// </summary>
/// <typeparam name="TDbContext">The type of database context.</typeparam>
public abstract class DataIntegrationTest<TDbContext> : IDisposable where TDbContext : DbContext
{
    private readonly List<IServiceScope> _additionalScopes = [];
    private readonly IDbContextConfigurator? _dbContextConfigurator;
    private readonly TestDbContextHarness<TDbContext>? _harness;
    private bool _disposed;
    private bool _teardownStarted;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataIntegrationTest{TDbContext}" /> class.
    /// </summary>
    /// <param name="dbContextConfigurator">
    ///     DbContext configurator to be used by the test. If not provided, then an in-memory SQLite database is used.
    /// </param>
    /// <param name="services">The service collection.</param>
    [SuppressMessage("Critical Code Smell", "S1699:Constructors should only call non-overridable methods", Justification = "It's fine in this context")]
    protected DataIntegrationTest(IDbContextConfigurator? dbContextConfigurator = null, IServiceCollection? services = null)
    {
        var serviceCollection = services ?? new ServiceCollection();

        // ReSharper disable once VirtualMemberCallInConstructor - this is not a problem here
        ConfigureServices(serviceCollection);

        dbContextConfigurator ??= new SqLiteDbContextConfigurator(SqLiteConnectionOptions.InMemory);
        _dbContextConfigurator = dbContextConfigurator;

        _harness = DbContextServicesRegistrationHelper.BuildHarness<TDbContext>(serviceCollection, dbContextConfigurator);
    }

    /// <summary>
    ///     Gets the configured instance of the database context.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the test harness has not been initialised — that is, when the
    ///     <see cref="DataIntegrationTest{TDbContext}" /> constructor has not completed.
    /// </exception>
    protected TDbContext DbContext => Harness.DbContext;

    /// <summary>
    ///     Provides access to the configured service provider.
    ///     This is used to resolve dependencies and services required during integration testing.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the test harness has not been initialised — that is, when the
    ///     <see cref="DataIntegrationTest{TDbContext}" /> constructor has not completed.
    /// </exception>
    protected IServiceProvider ScopedServiceProvider => Harness.ScopedServiceProvider;

    /// <summary>
    ///     Gets the root (non-scoped) service provider.
    /// </summary>
    /// <remarks>
    ///     Use this when you need to create additional scopes or resolve services
    ///     outside the default test scope. For most test code, prefer
    ///     <see cref="ScopedServiceProvider" /> instead.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the test harness has not been initialised — that is, when the
    ///     <see cref="DataIntegrationTest{TDbContext}" /> constructor has not completed.
    /// </exception>
    protected IServiceProvider RootServiceProvider => Harness.RootServiceProvider;

    private TestDbContextHarness<TDbContext> Harness =>
        _harness ??
        throw new InvalidOperationException("The test database harness has not been initialised. The DataIntegrationTest<TDbContext> constructor must " +
                                            "complete before the service providers or the database context are used.");

    /// <summary>
    ///     Disposes of the resources used by the current instance of the
    ///     <see cref="DataIntegrationTest{TDbContext}" /> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Creates a new dependency-injection scope from <see cref="RootServiceProvider" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The scope is tracked by this class and disposed when the test instance is disposed, so callers
    ///         do not have to dispose it themselves.
    ///     </para>
    ///     <para>
    ///         Use this when a test needs services with genuinely independent scoped lifetimes — a separate
    ///         <typeparamref name="TDbContext" /> with its own change tracker, for example — rather than the
    ///         instances shared through <see cref="ScopedServiceProvider" />.
    ///     </para>
    ///     <para>
    ///         Every scope is retained until the test instance is disposed; nothing is released early. A test
    ///         that calls this — directly, or indirectly through a <c>useScopedProvider: false</c> resolution —
    ///         in a long loop therefore holds one scope and one <typeparamref name="TDbContext" /> per iteration.
    ///         Dispose the returned scope yourself as well if that matters; disposal here is idempotent.
    ///     </para>
    ///     <para>
    ///         Tracking is synchronised, so this is safe to call concurrently from a test that fans out with
    ///         <see cref="Task.WhenAll(System.Collections.Generic.IEnumerable{Task})" /> or similar. Once
    ///         teardown has begun, a scope can no longer be registered — it would be created after the
    ///         disposal snapshot was taken and would therefore never be released — so this method throws
    ///         instead of handing back a scope nothing will clean up.
    ///     </para>
    /// </remarks>
    /// <returns>A new scope whose lifetime is bound to this test instance.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the test instance is being, or has been, disposed.</exception>
    protected IServiceScope CreateScope()
    {
        var scope = RootServiceProvider.CreateScope();

        lock (_additionalScopes)
        {
            if (!_teardownStarted)
            {
                _additionalScopes.Add(scope);

                return scope;
            }
        }

        // Registration lost the race with teardown. Release the orphan rather than leaking it, then fail
        // loudly: a caller that reaches this has a scope-creating operation outliving the test.
        scope.Dispose();

        throw new ObjectDisposedException(GetType().FullName,
                                          "Cannot create a dependency-injection scope: the integration test is being disposed, so the scope would never be released.");
    }

    /// <summary>
    ///     Creates a new <typeparamref name="TDbContext" /> instance from the root service provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use this when a test needs an additional context instance that is separate from
    ///         the default scoped <see cref="DbContext" /> exposed by this class.
    ///     </para>
    ///     <para>
    ///         The returned context should be disposed by the caller when no longer needed.
    ///     </para>
    ///     <example>
    ///         <code>
    ///         using var rootContext = CreateRootDbContext();
    ///         var total = await rootContext.Set&lt;MyEntity&gt;().CountAsync();
    ///         </code>
    ///     </example>
    /// </remarks>
    /// <returns>A <typeparamref name="TDbContext" /> resolved from <see cref="RootServiceProvider" />.</returns>
    protected TDbContext CreateRootDbContext()
    {
        var dbContextFactory = RootServiceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();

        return dbContextFactory.CreateDbContext();
    }

    /// <summary>
    ///     Configures the required services for the test.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method should be overridden in derived classes to configure additional
    ///         services required for the test.
    ///     </para>
    ///     <para>
    ///         By default, it registers the
    ///         <see cref="SqLiteDbContextCreationLifecycle" /> as the
    ///         <see cref="IDbContextCreationLifecycle" /> implementation, because
    ///         the test infrastructure defaults to an in-memory SQLite database.
    ///         This ensures the <c>DateTimeOffset</c> properties fix is applied
    ///         automatically.
    ///     </para>
    ///     <para>
    ///         If a derived class registers a different <see cref="IDbContextCreationLifecycle" />
    ///         before calling <c>base.ConfigureServices</c>, the existing registration
    ///         is preserved (this method uses <c>TryAddSingleton</c>).
    ///     </para>
    /// </remarks>
    /// <param name="services">The service collection.</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<IDbContextCreationLifecycle, SqLiteDbContextCreationLifecycle>();
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the <see cref="DataIntegrationTest{TDbContext}" />
    ///     and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///     true to release both managed and unmanaged resources; false to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            IServiceScope[] scopes;

            lock (_additionalScopes)
            {
                // Set inside the lock and before the snapshot, so a CreateScope() call that is already
                // waiting on this lock cannot append a scope after the snapshot has been taken and have it
                // silently escape disposal.
                _teardownStarted = true;
                scopes = [.. _additionalScopes];
                _additionalScopes.Clear();
            }

            // Dispose the scopes this test created before the harness, whose disposal cascades
            // through the root provider that owns them. A failing scope must not strand the harness
            // or the configurator, so each release is guarded and the failures are surfaced at the end.
            var failures = new List<Exception>();

            foreach (var scope in scopes)
            {
                TestDbContextHarness<TDbContext>.DisposeSafely(scope, failures);
            }

            // The harness owns the initial scope, the database context and the root provider,
            // and releases them in the correct order.
            TestDbContextHarness<TDbContext>.DisposeSafely(_harness, failures);

            // The harness never owns the configurator's connection, so the configurator is released here.
            TestDbContextHarness<TDbContext>.DisposeSafely(_dbContextConfigurator, failures);

            if (failures.Count == 1)
            {
                _disposed = true;

                throw failures[0];
            }

            if (failures.Count > 1)
            {
                _disposed = true;

                throw new AggregateException("Releasing the integration-test resources failed for more than one resource.", failures);
            }
        }

        _disposed = true;
    }
}
