using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore.Tests;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestEntity> TestEntities { get; set; } = null!;

    public DbSet<AnotherTestEntity> AnotherTestEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TestEntity>().HasKey(e => e.Id);
        modelBuilder.Entity<AnotherTestEntity>().HasKey(e => e.Id);
    }
}