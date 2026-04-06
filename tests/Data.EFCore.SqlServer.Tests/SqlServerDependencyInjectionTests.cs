using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.GenericRepository.EFCore.DependencyInjection;
using Ploch.Data.Model;

namespace Ploch.Data.EFCore.SqlServer.Tests;

public class SqlServerDependencyInjectionTests
{
    [Fact]
    public void AddDbContextWithRepositories_should_register_lifecycle_DbContext_and_repositories()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act — use a fake connection string; we won't actually connect
        services.AddDbContextWithRepositories<SqlServerDiTestDbContext>(() => "Server=localhost;Database=TestDb;Integrated Security=True;TrustServerCertificate=True");

        // Assert
        var provider = services.BuildServiceProvider();

        var lifecycle = provider.GetRequiredService<IDbContextCreationLifecycle>();
        lifecycle.Should().BeOfType<DefaultDbContextCreationLifecycle>();

        var dbContext = provider.GetRequiredService<SqlServerDiTestDbContext>();
        dbContext.Should().NotBeNull();

        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        unitOfWork.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextUsingSqlServer_should_register_default_lifecycle_and_DbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDbContextUsingSqlServer<SqlServerDiTestDbContext>(() => "Server=localhost;Database=TestDb;Integrated Security=True;TrustServerCertificate=True");

        // Assert
        var provider = services.BuildServiceProvider();

        var lifecycle = provider.GetRequiredService<IDbContextCreationLifecycle>();
        lifecycle.Should().BeOfType<DefaultDbContextCreationLifecycle>();

        var dbContext = provider.GetRequiredService<SqlServerDiTestDbContext>();
        dbContext.Should().NotBeNull();
    }

    [Fact]
    public void AddDbContextWithRepositories_should_throw_when_connection_string_is_null()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContextWithRepositories<SqlServerDiTestDbContext>(() => null);
        var provider = services.BuildServiceProvider();

        // Act — exception is thrown lazily when the DbContext is resolved
        var act = () => provider.GetRequiredService<SqlServerDiTestDbContext>();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*connection string*not found*");
    }

    public class SqlServerDiTestEntity : IHasId<int>
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }
    }

    public class SqlServerDiTestDbContext(DbContextOptions<SqlServerDiTestDbContext> options, IDbContextCreationLifecycle? lifecycle) : DbContext(options)
    {
        public DbSet<SqlServerDiTestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            lifecycle?.OnModelCreating(modelBuilder, Database);
            base.OnModelCreating(modelBuilder);
        }
    }
}
