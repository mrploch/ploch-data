using FluentAssertions;
using Ploch.Data.Model;
using Ploch.Data.Model.CommonTypes;
using Xunit;

namespace Ploch.Data.Model.Tests.CommonTypes;

public class PropertyTests
{
    [Fact]
    public void Id_should_be_settable_and_gettable()
    {
        var property = new Property<int, string> { Id = 1 };

        property.Id.Should().Be(1);
    }

    [Fact]
    public void Name_should_be_settable_and_gettable()
    {
        var property = new Property<int, string> { Name = "colour" };

        property.Name.Should().Be("colour");
    }

    [Fact]
    public void Value_should_be_settable_and_gettable()
    {
        var property = new Property<int, string> { Value = "blue" };

        property.Value.Should().Be("blue");
    }

    [Fact]
    public void Property_should_implement_IHasId()
    {
        var property = new Property<int, string>();

        property.Should().BeAssignableTo<IHasId<int>>();
    }

    [Fact]
    public void Property_should_implement_INamed()
    {
        var property = new Property<int, string>();

        property.Should().BeAssignableTo<INamed>();
    }

    [Fact]
    public void Property_should_implement_IHasValue()
    {
        var property = new Property<int, string>();

        property.Should().BeAssignableTo<IHasValue<string>>();
    }

    [Fact]
    public void PropertyWithDefaultId_should_use_int_id()
    {
        var property = new Property<string> { Id = 42, Name = "test", Value = "hello" };

        property.Id.Should().Be(42);
        property.Should().BeAssignableTo<IHasId<int>>();
    }
}

public class IntPropertyTests
{
    [Fact]
    public void IntProperty_should_have_int_value()
    {
        var property = new IntProperty { Id = 1, Name = "count", Value = 42 };

        property.Value.Should().Be(42);
        property.Should().BeAssignableTo<IHasValue<int>>();
    }

    [Fact]
    public void IntPropertyWithCustomId_should_support_custom_id_type()
    {
        var id = Guid.NewGuid();
        var property = new IntProperty<Guid> { Id = id, Name = "count", Value = 10 };

        property.Id.Should().Be(id);
        property.Value.Should().Be(10);
    }
}

public class StringPropertyTests
{
    [Fact]
    public void StringProperty_should_have_string_value()
    {
        var property = new StringProperty { Id = 1, Name = "label", Value = "hello" };

        property.Value.Should().Be("hello");
        property.Should().BeAssignableTo<IHasValue<string>>();
    }

    [Fact]
    public void StringPropertyWithCustomId_should_support_custom_id_type()
    {
        var property = new StringProperty<long> { Id = 999L, Name = "key", Value = "val" };

        property.Id.Should().Be(999L);
        property.Value.Should().Be("val");
    }
}
