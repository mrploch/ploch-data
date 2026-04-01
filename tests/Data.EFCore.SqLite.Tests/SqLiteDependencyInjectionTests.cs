using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;
using Ploch.Data.Model;
using Xunit;

namespace Ploch.Data.EFCore.SqLite.Tests;

public class SqLiteDependencyInjectionTests
{
    [Fact]
    public void AddDbContextWithRepositories_should_register_lifecycle_DbContext_and_repositories()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextWithRepositories<DiTestDbContext>(() => "Data Source=:memory:");

        // Assert
        var provider = services.BuildServiceProvider();

        var lifecycle = provider.GetRequiredService<IDbContextCreationLifecycle>();
        lifecycle.Should().BeOfType<SqLiteDbContextCreationLifecycle>();

        var dbContext = provider.GetRequiredService<DiTestDbContext>();
        dbContext.Should().NotBeNull();

        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        unitOfWork.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextWithRepositories_should_throw_when_connection_string_is_null()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContextWithRepositories<DiTestDbContext>(() => null);
        var provider = services.BuildServiceProvider();

        // Act
        var act = () => provider.GetRequiredService<DiTestDbContext>();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    public class DiTestEntity : IHasId<int>
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }

    public class DiTestDbContext : DbContext
    {
        private readonly IDbContextCreationLifecycle _lifecycle;

        public DiTestDbContext(DbContextOptions<DiTestDbContext> options, IDbContextCreationLifecycle lifecycle)
            : base(options) => _lifecycle = lifecycle;

        public DbSet<DiTestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _lifecycle.OnModelCreating(modelBuilder, Database);
            base.OnModelCreating(modelBuilder);
        }
    }
}
