using Microsoft.EntityFrameworkCore;

namespace Ploch.Common.Data.EFCore.Tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    { }

    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}