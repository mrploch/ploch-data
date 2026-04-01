using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ploch.Data.Model;
using Xunit;

namespace Ploch.Data.EFCore.SqLite.Tests;

public class SqLiteDbContextCreationLifecycleTests
{
    [Fact]
    public void OnModelCreating_should_apply_DateTimeOffset_fix_for_SqLite_provider()
    {
        // Arrange
        var lifecycle = new SqLiteDbContextCreationLifecycle();

        var connectionString = SqLiteConnectionOptions.InMemory.BuildConnectionString();
        var options = new DbContextOptionsBuilder<LifecycleTestDbContext>()
                     .UseSqlite(connectionString)
                     .Options;

        using var context = new LifecycleTestDbContext(options, lifecycle);

        // Act — force model creation by accessing the model
        var model = context.Model;

        // Assert — verify that DateTimeOffset properties have the binary converter
        var entityType = model.FindEntityType(typeof(LifecycleTestEntity))!;
        var createdTimeProperty = entityType.FindProperty(nameof(LifecycleTestEntity.CreatedTime))!;

        createdTimeProperty.GetValueConverter().Should().BeOfType<DateTimeOffsetToBinaryConverter>();
    }

    [Fact]
    public void OnConfiguring_should_not_throw()
    {
        // Arrange
        var lifecycle = new SqLiteDbContextCreationLifecycle();
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        var act = () => lifecycle.OnConfiguring(optionsBuilder);

        // Assert
        act.Should().NotThrow();
    }

    public class LifecycleTestEntity : IHasId<int>
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        public DateTimeOffset? CreatedTime { get; set; }
    }

    public class LifecycleTestDbContext : DbContext
    {
        private readonly IDbContextCreationLifecycle _lifecycle;

        public LifecycleTestDbContext(DbContextOptions<LifecycleTestDbContext> options, IDbContextCreationLifecycle lifecycle)
            : base(options) => _lifecycle = lifecycle;

        public DbSet<LifecycleTestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LifecycleTestDbContext).Assembly);
            _lifecycle.OnModelCreating(modelBuilder, Database);
            base.OnModelCreating(modelBuilder);
        }
    }
}
