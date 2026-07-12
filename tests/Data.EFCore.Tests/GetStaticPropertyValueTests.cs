using System.Diagnostics.CodeAnalysis;
using FluentAssertions;

namespace Ploch.Data.EFCore.Tests;

public class GetStaticPropertyValueTests
{
    [Fact]
    public void GetStaticPropertyValue_should_return_value_of_public_static_property()
    {
        var result = typeof(ClassWithStaticProperties).GetStaticPropertyValue<string>("PublicValue");

        result.Should().Be("public-value");
    }

    [Fact]
    public void GetStaticPropertyValue_should_return_value_of_private_static_property()
    {
        var result = typeof(ClassWithStaticProperties).GetStaticPropertyValue<int>("PrivateValue");

        result.Should().Be(42);
    }

    [Fact]
    public void GetStaticPropertyValue_should_throw_when_property_not_found()
    {
        var act = () => typeof(ClassWithStaticProperties).GetStaticPropertyValue<string>("NonExistent");

        act.Should().Throw<InvalidOperationException>().WithMessage("*'NonExistent'*not found*");
    }

    [Fact]
    public void GetStaticPropertyValue_should_return_default_when_property_value_is_null()
    {
        var result = typeof(ClassWithStaticProperties).GetStaticPropertyValue<string>("NullValue");

        result.Should().BeNull();
    }

    [Fact]
    public void GetStaticPropertyValue_should_throw_when_property_type_does_not_match()
    {
        var act = () => typeof(ClassWithStaticProperties).GetStaticPropertyValue<int>("PublicValue");

        act.Should().Throw<InvalidOperationException>().WithMessage("*not of*type*");
    }

    [SuppressMessage("Major Code Smell",
                     "S1144:Unused private types or members should be removed",
                     Justification = "All members are accessed via reflection through DbContextExtensions.GetStaticPropertyValue.")]
    private static class ClassWithStaticProperties
    {
        public static string PublicValue { get; } = "public-value";

        public static string? NullValue { get; }

        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("CodeQuality",
                         "IDE0051:Remove unused private members",
                         Justification = "Accessed via reflection by GetStaticPropertyValue_should_return_value_of_private_static_property.")]
        private static int PrivateValue { get; } = 42;
    }
}
