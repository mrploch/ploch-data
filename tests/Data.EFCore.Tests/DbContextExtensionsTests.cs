using FluentAssertions;
using Ploch.Data.EFCore.IntegrationTesting;

namespace Ploch.Data.EFCore.Tests;

public class DbContextExtensionsTests : DataIntegrationTest<TestDbContext>
{
    [Fact]
    public void Set_should_return_DbSet_for_a_given_name()
    {
        var dbSet = DbContext.Set("AnotherTestEntity");

        dbSet.Should().BeSameAs(DbContext.AnotherTestEntities);
    }
}