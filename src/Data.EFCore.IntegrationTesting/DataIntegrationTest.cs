using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore;
using Ploch.Data.EFCore.SqLite;

namespace Ploch.Data.EFCore.IntegrationTesting;

/// <summary>
///     Abstract base class for integration tests that involve Entity Framework Core.
///     Provides initialization and configuration of the database context and services.
/// </summary>
/// <typeparam name="TDbContext">The type of database context.</typeparam>
public abstract class DataIntegrationTest<TDbContext> : IDisposable where TDbContext : DbContext
{
    private readonly IDbContextConfigurator? _dbContextConfigurator;

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

        (ServiceProvider, DbContext) =
            DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TDbContext>(serviceCollection, dbContextConfigurator);
    }

    /// <summary>
    ///     Gets the configured instance of the database context.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Part of the public API.")]
    protected TDbContext DbContext { get; }

    /// <summary>
    ///     Provides access to the configured service provider.
    ///     This is used to resolve dependencies and services required during integration testing.
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

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
    ///     Configures the required services for the test.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method should be overridden in derived classes to configure additional
    ///         services required for the test.
    ///     </para>
    ///     <para>
    ///         By default, it registers the <see cref="DefaultDbContextCreationLifecycle" />
    ///         as the <see cref="IDbContextCreationLifecycle" /> implementation.
    ///         Override this method and register a different implementation if your
    ///         <see cref="DbContext" /> requires provider-specific lifecycle behaviour.
    ///     </para>
    /// </remarks>
    /// <param name="services">The service collection.</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddDefaultDbContextCreationLifecycle();
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
        if (disposing)
        {
            DbContext.Dispose();

            if (ServiceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }

            if (_dbContextConfigurator is IDisposable disposableConfigurator)
            {
                disposableConfigurator.Dispose();
            }
        }
    }
}
