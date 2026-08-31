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

        var (rootServiceProvider, dbContext) = CreateDbContext();

        using (rootServiceProvider)
        using (dbContext)
        {
            dbContext.TestEntities.ExecuteDelete();

            var dataSeeder = new TestDataSeeder(dbContext);
            dataSeeder.Execute();

            var entities = dbContext.TestEntities.AsNoTracking().ToList();
            entities.Should().HaveCount(2);
            entities.Should().ContainSingle(e => e.Name == "Test1");
            entities.Should().ContainSingle(e => e.Name == "Test2");
        }
    }

    [Fact]
    public void DataContext_should_use_the_explicitly_created_database()
    {
        fixture.SkipIfUnavailable();

        var (rootServiceProvider, dbContext) = CreateDbContext();

        using (rootServiceProvider)
        using (dbContext)
        {
            // Connecting with an Initial Catalog that does not exist fails, so reaching this point at
            // all proves the fixture created the database before the DbContext opened its connection.
            dbContext.Database.GetDbConnection().Database.Should().Be(SqlServerContainerFixture.DatabaseName);
            dbContext.Database.CanConnect().Should().BeTrue();
        }
    }

    private (ServiceProvider RootServiceProvider, TestDbContext DbContext) CreateDbContext()
    {
        var configurator = new SqlServerDbContextConfigurator(fixture.ConnectionString, builder => builder.EnableRetryOnFailure());

        var (rootServiceProvider, _, dbContext) =
            DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider<TestDbContext>(new ServiceCollection(), configurator);

        return ((ServiceProvider)rootServiceProvider, dbContext);
    }
}
