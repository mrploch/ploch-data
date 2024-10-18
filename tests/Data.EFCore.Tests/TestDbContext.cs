using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore.Tests;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
}