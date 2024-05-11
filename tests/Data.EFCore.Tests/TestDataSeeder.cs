namespace Ploch.Common.Data.EFCore.Tests;

public class TestDataSeeder : DataSeeder<TestDbContext>
{
    public TestDataSeeder(TestDbContext dbContext) : base(dbContext)
    { }

    protected override void InitializeData()
    {
        DbContext.TestEntities.Add(new TestEntity { Name = "Test1" });
        DbContext.TestEntities.Add(new TestEntity { Name = "Test2" });
    }
}