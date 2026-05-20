using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;

namespace Ploch.Data.EFCore.Tests;

public class DefaultDbContextCreationLifecycleTests
{
    [Fact]
    public void OnModelCreating_should_not_throw()
    {
        // Arrange
        var lifecycle = new DefaultDbContextCreationLifecycle();
        var modelBuilder = new ModelBuilder();
        var database = new Mock<DatabaseFacade>(new Mock<DbContext>().Object).Object;

        // Act
        var act = () => lifecycle.OnModelCreating(modelBuilder, database);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void OnConfiguring_should_not_throw()
    {
        // Arrange
        var lifecycle = new DefaultDbContextCreationLifecycle();
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        var act = () => lifecycle.OnConfiguring(optionsBuilder);

        // Assert
        act.Should().NotThrow();
    }
}
