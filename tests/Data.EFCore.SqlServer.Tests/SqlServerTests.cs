using FluentAssertions;
using Ploch.Data.EFCore.IntegrationTesting;
using Ploch.Data.EFCore.Tests;

namespace Ploch.Data.EFCore.SqlServer.Tests;

public class SqlServerTests : DataIntegrationTest<TestDbContext>
{
    public SqlServerTests() : base(new SqlServerDbContextConfigurator(builder =>
                                                                      {
                                                                          builder.DataSource = "localhost";
                                                                          builder.InitialCatalog = $"Ploch.Data.Tests.{Guid.NewGuid()}";
                                                                          builder.UserID = "sa";
                                                                          builder.Password = "P@ssw0rd";
                                                                          builder.TrustServerCertificate = true;
                                                                      },
                                                                      builder => builder.EnableRetryOnFailure()))
    {
    }

    [Fact]
    public void DataContext_should_be_functional()
    {
        var dataSeeder = new TestDataSeeder(DbContext);
        dataSeeder.Execute();

        var entities = DbContext.TestEntities.ToList();
        entities.Should().HaveCount(2);
        entities.Should().ContainSingle(e => e.Name == "Test1");
        entities.Should().ContainSingle(e => e.Name == "Test2");
    }
}