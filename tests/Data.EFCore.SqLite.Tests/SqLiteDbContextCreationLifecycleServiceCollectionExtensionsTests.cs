using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
}
