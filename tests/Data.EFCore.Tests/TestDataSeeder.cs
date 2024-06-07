namespace Ploch.Common.Data.EFCore.Tests;

public class TestDataSeeder(TestDbContext dbContext) : DataSeeder<TestDbContext>(dbContext)
{
    protected override void InitializeData()
    {
        DbContext.TestEntities.Add(new TestEntity { Name = "Test1" });
        DbContext.TestEntities.Add(new TestEntity { Name = "Test2" });
    }
}