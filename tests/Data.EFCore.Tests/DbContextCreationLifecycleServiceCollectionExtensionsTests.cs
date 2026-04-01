using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.Model;

namespace Ploch.Data.EFCore.Tests;

public class DbContextCreationLifecycleServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDefaultDbContextCreationLifecycle_should_register_DefaultDbContextCreationLifecycle()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDefaultDbContextCreationLifecycle();

        // Assert
        var provider = services.BuildServiceProvider();
        var lifecycle = provider.GetRequiredService<IDbContextCreationLifecycle>();
        lifecycle.Should().BeOfType<DefaultDbContextCreationLifecycle>();
    }

    [Fact]
    public void AddDefaultDbContextCreationLifecycle_should_register_as_singleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDefaultDbContextCreationLifecycle();

        // Assert
        var descriptor = services.Single(d => d.ServiceType == typeof(IDbContextCreationLifecycle));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDbContextWithDefaultCreationLifecycle_should_register_lifecycle_and_DbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextWithDefaultCreationLifecycle<LifecycleTestDbContext>(
            options => options.UseSqlite("Data Source=:memory:"));

        // Assert
        var provider = services.BuildServiceProvider();
        var lifecycle = provider.GetRequiredService<IDbContextCreationLifecycle>();
        lifecycle.Should().BeOfType<DefaultDbContextCreationLifecycle>();

        var dbContext = provider.GetRequiredService<LifecycleTestDbContext>();
        dbContext.Should().NotBeNull();
    }

    public class LifecycleTestEntity : IHasId<int>
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }

    public class LifecycleTestDbContext(DbContextOptions<LifecycleTestDbContext> options, IDbContextCreationLifecycle lifecycle)
        : DbContext(options)
    {
        public DbSet<LifecycleTestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            lifecycle.OnModelCreating(modelBuilder, Database);
            base.OnModelCreating(modelBuilder);
        }
    }
}
