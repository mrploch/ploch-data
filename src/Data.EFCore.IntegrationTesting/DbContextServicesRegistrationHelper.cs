using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ploch.Data.EFCore.IntegrationTesting;

/// <summary>
///     Helper class for registering and configuring DbContext services for integration testing.
/// </summary>
public static class DbContextServicesRegistrationHelper
{
    /// <summary>
    ///     Builds a <see cref="TestDbContextHarness{TDbContext}" /> that owns the root service provider, the initial
    ///     scope, the shared SQLite connection and the prepared database context.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Prefer this over <see cref="BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,string)" />: the
    ///         harness is a single <see cref="IDisposable" /> that releases every one of those resources, whereas the tuple
    ///         leaves ownership to the caller.
    ///     </para>
    ///     <para>
    ///         This overload registers both <c>DbContext</c> and <c>IDbContextFactory</c> against the shared connection, so
    ///         a harness built here supports factory-based helpers such as
    ///         <see cref="DataIntegrationTest{TDbContext}.CreateRootDbContext" />.
    ///     </para>
    ///     <para>
    ///         The connection created here is owned by the returned harness, and is released when the harness is disposed.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="connectionString">The database connection string. Default is an in-memory SQLite database.</param>
    /// <returns>A harness owning every resource created for the test.</returns>
    public static TestDbContextHarness<TDbContext> BuildHarness<TDbContext>(IServiceCollection serviceCollection,
                                                                           string connectionString = "Data Source=:memory:") where TDbContext : DbContext
    {
        // Create the connection once and share it across all DbContext instances.
        // This is critical for SQLite in-memory databases: each new connection to :memory:
        // creates a separate empty database, so all consumers must share a single connection.
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        serviceCollection.AddSingleton(connection);
        serviceCollection.AddDbContext<TDbContext>(builder => builder.UseSqlite(connection));

        // Registered for parity with the configurator overload so that factory-based helpers work
        // identically whichever overload built the harness.
        serviceCollection.AddDbContextFactory<TDbContext>(builder => builder.UseSqlite(connection));

        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection, connection);
    }

    /// <summary>
    ///     Builds a <see cref="TestDbContextHarness{TDbContext}" /> using a custom DbContext configurator.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Prefer this over
    ///         <see cref="BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,IDbContextConfigurator)" />: the
    ///         harness is a single <see cref="IDisposable" /> that releases the root provider, the initial scope and the
    ///         database context, whereas the tuple leaves ownership to the caller.
    ///     </para>
    ///     <para>
    ///         <strong>The database connection is not owned by the harness on this path.</strong> It belongs to
    ///         <paramref name="dbContextConfigurator" />, so a caller using a configurator that owns a shared connection —
    ///         <c>SqLiteDbContextConfigurator</c>, for instance — must dispose the configurator in addition to the harness.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="dbContextConfigurator">The configurator responsible for setting up the DbContext options.</param>
    /// <returns>A harness owning every resource created for the test, except the configurator and its connection.</returns>
    public static TestDbContextHarness<TDbContext> BuildHarness<TDbContext>(IServiceCollection serviceCollection,
                                                                           IDbContextConfigurator dbContextConfigurator) where TDbContext : DbContext
    {
        serviceCollection.AddDbContext<TDbContext>(dbContextConfigurator.Configure);
        serviceCollection.AddDbContextFactory<TDbContext>(dbContextConfigurator.Configure);

        // The connection, if any, belongs to the configurator, so the harness must not dispose it.
        return CreateProviderAndPrepareDbContext<TDbContext>(serviceCollection, connection: null);
    }

    /// <inheritdoc cref="BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,IDbContextConfigurator)" />
    /// <summary>
    ///     Builds a DbContext and IServiceProvider for integration testing.
    /// </summary>
    /// <remarks>
    ///     This overload hands back three references but no ownership, and deliberately does not expose the
    ///     <see cref="TestDbContextHarness{TDbContext}" /> it is built on — see the remarks on
    ///     <see cref="BuildDbContextAndServiceProvider{TDbContext}(IServiceCollection,IDbContextConfigurator)" />.
    ///     The caller must dispose <c>ScopedProvider</c> and then <c>RootProvider</c>; disposing the root alone does not
    ///     release the child scope. Use <see cref="BuildHarness{TDbContext}(IServiceCollection,string)" /> when
    ///     ownership matters.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="connectionString">The database connection string. Default is an in-memory SQLite database.</param>
    public static (IServiceProvider RootProvider, IServiceProvider ScopedProvider, TDbContext DbContext) BuildDbContextAndServiceProvider<TDbContext>(
        IServiceCollection serviceCollection,
        string connectionString = "Data Source=:memory:") where TDbContext : DbContext
    {
        var (rootProvider, scopedProvider, dbContext) = BuildHarness<TDbContext>(serviceCollection, connectionString);

        return (rootProvider, scopedProvider, dbContext);
    }

    /// <summary>
    ///     Builds a DbContext and IServiceProvider for integration testing using a custom DbContext configurator.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This overload hands back three references but no ownership: the caller remains responsible for disposing
    ///         the scope behind <c>ScopedProvider</c> and then the root provider, in that order. Prefer
    ///         <see cref="BuildHarness{TDbContext}(IServiceCollection,IDbContextConfigurator)" />, which owns all of them.
    ///     </para>
    ///     <para>
    ///         The <see cref="TestDbContextHarness{TDbContext}" /> this overload is implemented on is intentionally not
    ///         returned: exposing it would change the signature these callers depend on. The consequence is that cleanup
    ///         is entirely the caller's job, and it takes two disposals in order — the root provider does <em>not</em>
    ///         track the child scope it created, so disposing <c>RootProvider</c> alone leaves the scoped
    ///         <typeparamref name="TDbContext" /> undisposed. Dispose <c>ScopedProvider</c> (cast to
    ///         <see cref="IDisposable" />) first, then <c>RootProvider</c>. Callers who would rather not remember that
    ///         should call <see cref="BuildHarness{TDbContext}(IServiceCollection,IDbContextConfigurator)" /> directly,
    ///         which orders the whole chain for them.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the DbContext to configure.</typeparam>
    /// <param name="serviceCollection">The service collection to which the DbContext is added.</param>
    /// <param name="dbContextConfigurator">The configurator responsible for setting up the DbContext options.</param>
    /// <returns>
    ///     A tuple containing the root IServiceProvider (<c>RootProvider</c>), the scoped IServiceProvider (<c>ScopedProvider</c>), the configured TDbContext (
    ///     <c>DbContext</c>).
    /// </returns>
    public static (IServiceProvider RootProvider, IServiceProvider ScopedProvider, TDbContext DbContext) BuildDbContextAndServiceProvider<TDbContext>(
        IServiceCollection serviceCollection,
        IDbContextConfigurator dbContextConfigurator) where TDbContext : DbContext
    {
        var (rootProvider, scopedProvider, dbContext) = BuildHarness<TDbContext>(serviceCollection, dbContextConfigurator);

        return (rootProvider, scopedProvider, dbContext);
    }

    private static TestDbContextHarness<TDbContext> CreateProviderAndPrepareDbContext<TDbContext>(IServiceCollection serviceCollection, SqliteConnection? connection)
        where TDbContext : DbContext
    {
        ServiceProvider? serviceProvider = null;
        IServiceScope? scope = null;
        TDbContext? testDbContext = null;

        try
        {
            serviceProvider = serviceCollection.BuildServiceProvider();
            scope = serviceProvider.CreateScope();
            testDbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            testDbContext.Database.OpenConnection();
            testDbContext.Database.EnsureCreated();

            // The harness exposes the scoped service provider so that repositories resolved by tests
            // share the same DbContext instance (and its change tracker).
            // The shared connection in SqLiteDbContextConfigurator ensures all DbContext instances
            // (including those in UnitOfWork child scopes) access the same in-memory database.
            return new TestDbContextHarness<TDbContext>(serviceProvider, scope, testDbContext, connection);
        }
        catch
        {
            // Construction failed part-way — most commonly in EnsureCreated() on a bad model — so nothing
            // will ever receive the harness that would have released these. Release what was created, in the
            // same order the harness uses. Failures here are collected and dropped on purpose: the exception
            // that aborted construction is the useful one and must not be masked by a cleanup failure.
            var cleanupFailures = new List<Exception>();

            TestDbContextHarness<TDbContext>.DisposeSafely(testDbContext, cleanupFailures);
            TestDbContextHarness<TDbContext>.DisposeSafely(scope, cleanupFailures);
            TestDbContextHarness<TDbContext>.DisposeSafely(serviceProvider, cleanupFailures);

            // Null on the configurator path, where the connection belongs to the configurator.
            TestDbContextHarness<TDbContext>.DisposeSafely(connection, cleanupFailures);

            throw;
        }
    }
}
