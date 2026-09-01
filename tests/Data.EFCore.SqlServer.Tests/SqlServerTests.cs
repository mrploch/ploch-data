using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.EFCore.Tests;

namespace Ploch.Data.EFCore.SqlServer.Tests;

/// <summary>
///     Integration tests exercising the SQL Server provider against a throw-away container.
/// </summary>
/// <param name="fixture">The SQL Server container fixture.</param>
public class SqlServerTests(SqlServerContainerFixture fixture) : IClassFixture<SqlServerContainerFixture>
{
    [Fact]
    public void DataContext_should_be_functional()
    {
        fixture.SkipIfUnavailable();

        var (rootServiceProvider, scopedServiceProvider, dbContext) = CreateDbContext();

        try
        {
            dbContext.TestEntities.ExecuteDelete();

            var dataSeeder = new TestDataSeeder(dbContext);
            dataSeeder.Execute();

            // The seeding context must not be used for the assertion - a fresh context proves the
            // rows reached the database instead of being served from the change tracker.
            using var verificationDbContext = CreateVerificationDbContext(rootServiceProvider);

            var entities = verificationDbContext.TestEntities.ToList();
            entities.Should().HaveCount(2);
            entities.Should().ContainSingle(e => e.Name == "Test1");
            entities.Should().ContainSingle(e => e.Name == "Test2");
        }
        finally
        {
            DisposeAll(dbContext, scopedServiceProvider, rootServiceProvider);
        }
    }

    [Fact]
    public void DataContext_should_use_the_explicitly_created_database()
    {
        fixture.SkipIfUnavailable();

        var (rootServiceProvider, scopedServiceProvider, dbContext) = CreateDbContext();

        try
        {
            // Ask the server which catalog the session is actually bound to, rather than reading the
            // Initial Catalog back out of the connection string the fixture just built.
            var databaseName = dbContext.Database.SqlQuery<string>($"SELECT DB_NAME() AS Value").Single();

            databaseName.Should().Be(SqlServerContainerFixture.DatabaseName);
        }
        finally
        {
            DisposeAll(dbContext, scopedServiceProvider, rootServiceProvider);
        }
    }

    private static TestDbContext CreateVerificationDbContext(IServiceProvider rootServiceProvider)
    {
        return rootServiceProvider.GetRequiredService<IDbContextFactory<TestDbContext>>().CreateDbContext();
    }

    /// <summary>
    ///     Disposes the supplied objects that implement <see cref="IDisposable" />, in the order given.
    /// </summary>
    /// <remarks>
    ///     <see cref="IServiceProvider" /> does not implement <see cref="IDisposable" />, so the concrete
    ///     provider and scope are probed rather than cast - mirroring
    ///     <c>DataIntegrationTest.Dispose(bool)</c>.
    /// </remarks>
    private static void DisposeAll(params object?[] candidates)
    {
        foreach (var disposable in candidates.OfType<IDisposable>())
        {
            disposable.Dispose();
        }
    }

    private (IServiceProvider RootServiceProvider, IServiceProvider ScopedServiceProvider, TestDbContext DbContext) CreateDbContext()
    {
        var configurator = new SqlServerDbContextConfigurator(fixture.ConnectionString, builder => builder.EnableRetryOnFailure());

        return DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(new ServiceCollection(), configurator);
    }
}
