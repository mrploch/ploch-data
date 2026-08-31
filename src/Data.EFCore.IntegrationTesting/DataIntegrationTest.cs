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
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Part of the public API.")]
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
    /// </remarks>
    /// <returns>A new scope whose lifetime is bound to this test instance.</returns>
    protected IServiceScope CreateScope()
    {
        var scope = RootServiceProvider.CreateScope();
        _additionalScopes.Add(scope);

        return scope;
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
            // Dispose the scopes this test created before the harness, whose disposal cascades
            // through the root provider that owns them.
            foreach (var scope in _additionalScopes)
            {
                scope.Dispose();
            }

            _additionalScopes.Clear();

            // The harness owns the initial scope, the database context and the root provider,
            // and releases them in the correct order.
            _harness?.Dispose();

            if (_dbContextConfigurator is IDisposable disposableConfigurator)
            {
                disposableConfigurator.Dispose();
            }
        }

        _disposed = true;
    }
}
