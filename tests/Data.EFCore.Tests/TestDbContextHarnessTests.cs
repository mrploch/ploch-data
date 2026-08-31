using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore.IntegrationTesting;

namespace Ploch.Data.EFCore.Tests;

public class TestDbContextHarnessTests
{
    [Fact]
    public void BuildHarness_should_expose_a_usable_database_context_and_providers()
    {
        // Arrange & Act
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
        // Arrange & Act
        var (rootProvider, scopedProvider, dbContext) = DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(new ServiceCollection());

        // Assert
        rootProvider.Should().NotBeNull();
        scopedProvider.Should().NotBeSameAs(rootProvider);
        dbContext.Should().BeSameAs(scopedProvider.GetRequiredService<TestDbContext>());

        ((IDisposable)rootProvider).Dispose();
    }
}
