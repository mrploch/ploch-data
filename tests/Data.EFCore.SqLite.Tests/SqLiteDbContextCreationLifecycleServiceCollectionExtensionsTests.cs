using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.Model;
using Xunit;

namespace Ploch.Data.EFCore.SqLite.Tests;

public class SqLiteDbContextCreationLifecycleServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSqLiteDbContextCreationLifecycle_should_register_SqLiteDbContextCreationLifecycle()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSqLiteDbContextCreationLifecycle();

        // Assert
        var provider = services.BuildServiceProvider();
        var lifecycle = provider.GetRequiredService<IDbContextCreationLifecycle>();
        lifecycle.Should().BeOfType<SqLiteDbContextCreationLifecycle>();
    }

    [Fact]
    public void AddSqLiteDbContextCreationLifecycle_should_register_as_singleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSqLiteDbContextCreationLifecycle();

        // Assert
        var descriptor = services.Single(d => d.ServiceType == typeof(IDbContextCreationLifecycle));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDbContextWithSqLiteCreationLifecycle_should_register_lifecycle_and_DbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextWithSqLiteCreationLifecycle<SqLiteLifecycleTestDbContext>(
            () => "Data Source=:memory:");

        // Assert
        var provider = services.BuildServiceProvider();
        var lifecycle = provider.GetRequiredService<IDbContextCreationLifecycle>();
        lifecycle.Should().BeOfType<SqLiteDbContextCreationLifecycle>();

        var dbContext = provider.GetRequiredService<SqLiteLifecycleTestDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextWithSqLiteCreationLifecycle_should_throw_when_connection_string_is_null()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContextWithSqLiteCreationLifecycle<SqLiteLifecycleTestDbContext>(() => null);
        var provider = services.BuildServiceProvider();

        // Act
        var act = () => provider.GetRequiredService<SqLiteLifecycleTestDbContext>();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    public class SqLiteLifecycleTestEntity : IHasId<int>
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }

    public class SqLiteLifecycleTestDbContext : DbContext
    {
        private readonly IDbContextCreationLifecycle _lifecycle;

        public SqLiteLifecycleTestDbContext(DbContextOptions<SqLiteLifecycleTestDbContext> options, IDbContextCreationLifecycle lifecycle)
            : base(options) => _lifecycle = lifecycle;

        public DbSet<SqLiteLifecycleTestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _lifecycle.OnModelCreating(modelBuilder, Database);
            base.OnModelCreating(modelBuilder);
        }
    }
}
