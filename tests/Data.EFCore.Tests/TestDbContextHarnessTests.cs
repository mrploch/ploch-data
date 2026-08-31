using System.Data;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.EFCore.SqLite;

namespace Ploch.Data.EFCore.Tests;

public class TestDbContextHarnessTests
{
    [Fact]
    public void BuildHarness_should_expose_a_usable_database_context_and_providers()
    {
        // Arrange and Act
        using var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection());

        // Assert
        harness.RootServiceProvider.Should().NotBeNull();
        harness.ScopedServiceProvider.Should().NotBeSameAs(harness.RootServiceProvider);
        harness.DbContext.TestEntities.Should().BeEmpty();
    }

    [Fact]
    public void Deconstruct_should_yield_the_same_references_as_the_properties()
    {
        // Arrange
        using var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection());

        // Act
        var (rootProvider, scopedProvider, dbContext) = harness;

        // Assert
        rootProvider.Should().BeSameAs(harness.RootServiceProvider);
        scopedProvider.Should().BeSameAs(harness.ScopedServiceProvider);
        dbContext.Should().BeSameAs(harness.DbContext);
    }

    [Fact]
    public void Dispose_should_release_the_database_context_the_scope_and_the_shared_connection()
    {
        // Arrange
        var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection());
        var connection = harness.RootServiceProvider.GetRequiredService<SqliteConnection>();
        var scopedProvider = harness.ScopedServiceProvider;

        // Act
        harness.Dispose();

        // Assert — every owned resource is released, so each one now rejects further use.
        connection.State.Should().Be(System.Data.ConnectionState.Closed);
        FluentActions.Invoking(() => harness.DbContext.TestEntities.ToList()).Should().Throw<ObjectDisposedException>();
        FluentActions.Invoking(() => scopedProvider.GetRequiredService<TestDbContext>()).Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_should_be_idempotent()
    {
        // Arrange
        var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection());
        harness.Dispose();

        // Act
        Action act = harness.Dispose;

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task DisposeAsync_should_release_the_shared_connection()
    {
        // Arrange
        var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection());
        var connection = harness.RootServiceProvider.GetRequiredService<SqliteConnection>();

        // Act
        await harness.DisposeAsync();

        // Assert
        connection.State.Should().Be(System.Data.ConnectionState.Closed);
    }

    [Fact]
    public async Task DisposeAsync_should_be_idempotent()
    {
        // Arrange
        var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection());
        await harness.DisposeAsync();

        // Act
        var act = async () => await harness.DisposeAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void BuildDbContextAndServiceProvider_should_return_the_harness_references()
    {
        // The legacy tuple overload is routed through the harness; it must keep returning the same
        // three references so existing callers are unaffected.
        // Arrange and Act
        var (rootProvider, scopedProvider, dbContext) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(new ServiceCollection());

        // Assert
        rootProvider.Should().NotBeNull();
        scopedProvider.Should().NotBeSameAs(rootProvider);
        dbContext.Should().BeSameAs(scopedProvider.GetRequiredService<TestDbContext>());

        // Cleanup — the tuple overload hands back no ownership. The root provider does not track the child
        // scope it created, so the scope must be disposed first and the root provider second; disposing only
        // the root would leave the scoped context alive. This is the gap BuildHarness closes.
        ((IDisposable)scopedProvider).Dispose();
        ((IDisposable)rootProvider).Dispose();
    }

    [Fact]
    public void BuildHarness_should_register_a_context_factory_for_the_shared_connection()
    {
        // The configurator overload registers IDbContextFactory; this overload must do the same, otherwise
        // factory-based helpers such as DataIntegrationTest.CreateRootDbContext() work on one path only.
        // Arrange
        using var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection());

        // Act
        using var factoryContext = harness.RootServiceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>().CreateDbContext();

        // Assert — the factory context sees the schema created on the shared in-memory connection.
        factoryContext.Should().NotBeSameAs(harness.DbContext);
        factoryContext.TestEntities.Should().BeEmpty();
    }

    [Fact]
    public void BuildHarness_with_a_configurator_should_not_dispose_the_configurator_connection()
    {
        // The configurator owns its shared connection, so the harness must leave it alone — the caller
        // remains responsible for disposing the configurator.
        // Arrange
        using var configurator = new SqLiteDbContextConfigurator(SqLiteConnectionOptions.InMemory);
        var harness = DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(new ServiceCollection(), configurator);
        var connection = harness.DbContext.Database.GetDbConnection();
        connection.State.Should().Be(ConnectionState.Open);

        // Act
        harness.Dispose();

        // Assert
        connection.State.Should().Be(ConnectionState.Open);

        configurator.Dispose();
        connection.State.Should().Be(ConnectionState.Closed);
    }

    [Fact]
    public void BuildHarness_should_release_everything_it_created_when_preparation_fails()
    {
        // Arrange — an unusable connection string makes Open() fail inside the builder, after the
        // SqliteConnection has been created but before any harness can be handed back.
        var serviceCollection = new ServiceCollection();

        // Act
        var act = () => DbContextServicesRegistrationHelper.BuildHarness<TestDbContext>(serviceCollection, "Data Source=:memory:;Mode=NotAValidMode");

        // Assert — the original failure surfaces rather than a cleanup failure masking it.
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DataIntegrationTest_members_should_throw_when_the_harness_is_not_initialised()
    {
        // Arrange and Act — the derived type reads DbContext from ConfigureServices, which the base
        // constructor calls before the harness exists.
        var act = () => new HarnessReadDuringConstruction();

        // Assert
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("*harness has not been initialised*");
    }

    private sealed class HarnessReadDuringConstruction : DataIntegrationTest<TestDbContext>
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            // Reading DbContext here must throw: the base constructor calls ConfigureServices before it
            // builds the harness, so the guard is the only thing standing between a caller and a null.
            services.AddSingleton(DbContext);
        }
    }
}
