using FluentAssertions;
using Ploch.Data.EFCore.IntegrationTesting;

namespace Ploch.Common.Data.EFCore.Tests;

public class DataSeederTests : DataIntegrationTest<TestDbContext>
{
    [Fact]
    public void TestDataSeeder_should_initialize_data_in_TestDbContext()
    {
        var dataSeeder = new TestDataSeeder(DbContext);
        dataSeeder.Execute();

        var entities = DbContext.TestEntities.ToList();
        entities.Should().HaveCount(2);
        entities.Should().ContainSingle(e => e.Name == "Test1");
        entities.Should().ContainSingle(e => e.Name == "Test2");
    }
}