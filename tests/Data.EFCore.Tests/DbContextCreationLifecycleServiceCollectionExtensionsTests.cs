using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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
}
