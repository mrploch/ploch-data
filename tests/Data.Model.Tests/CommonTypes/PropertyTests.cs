using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutoFixture.Xunit3;
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

    [Theory]
    [AutoData]
    public void Name_should_round_trip_an_arbitrary_value(string name)
    {
        var property = new Property<int, string> { Name = name };

        property.Name.Should().Be(name);
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

    [Fact]
    public void Name_should_be_null_on_a_newly_constructed_property()
    {
        // Name is declared as a non-nullable string initialised with the null-forgiving `= null!`,
        // which silences the compiler for the EF Core materialisation path but leaves the property
        // genuinely null until something assigns it. Pinning that here so the discrepancy between
        // the declared type and the runtime value is a documented contract, not a surprise.
        var property = new Property<int, string>();

        property.Name.Should().BeNull();
    }

    [Fact]
    public void Value_should_be_null_on_a_newly_constructed_property_with_a_reference_type_value()
    {
        var property = new Property<int, string>();

        property.Value.Should().BeNull();
    }

    [Fact]
    public void Value_should_be_the_type_default_on_a_newly_constructed_property_with_a_value_type_value()
    {
        var property = new Property<int, int>();

        property.Value.Should().Be(0);
    }

    [Fact]
    public void Id_should_be_the_type_default_on_a_newly_constructed_property_with_a_value_type_id()
    {
        var property = new Property<Guid, string>();

        property.Id.Should().BeEmpty();
    }

    [Fact]
    public void Id_should_be_null_on_a_newly_constructed_property_with_a_reference_type_id()
    {
        var property = new Property<string, string>();

        property.Id.Should().BeNull();
    }

    [Fact]
    public void Name_should_accept_null()
    {
        var property = new Property<int, string> { Name = "colour" };
        property.Name.Should().Be("colour");

        property.Name = null!;

        property.Name.Should().BeNull();
    }

    [Fact]
    public void Name_should_accept_whitespace()
    {
        // Property is a plain data carrier — it deliberately enforces no invariants on Name, so
        // whitespace and empty strings are accepted. If validation is ever introduced this test
        // fails, forcing the change to be a conscious one rather than a silent behavioural break.
        var property = new Property<int, string> { Name = "   " };

        property.Name.Should().Be("   ");
    }

    [Fact]
    public void Value_should_accept_null_for_a_reference_type()
    {
        var property = new Property<int, string> { Value = "blue" };
        property.Value.Should().Be("blue");

        property.Value = null!;

        property.Value.Should().BeNull();
    }

    [Fact]
    public void Value_should_round_trip_null_for_a_nullable_value_type()
    {
        var property = new Property<int, int?> { Value = 42 };
        property.Value.Should().Be(42);

        property.Value = null;

        property.Value.Should().BeNull();
    }

    [Fact]
    public void Property_should_support_a_reference_type_id()
    {
        var property = new Property<string, int> { Id = "colour-key", Name = "colour", Value = 7 };

        property.Id.Should().Be("colour-key");
        property.Should().BeAssignableTo<IHasId<string>>();
    }

    [Fact]
    public void Property_should_support_a_collection_value()
    {
        var values = new List<int> { 1, 2, 3 };
        var property = new Property<string, List<int>> { Id = "sizes", Name = "sizes", Value = values };

        property.Value.Should().BeSameAs(values);
    }

    [Fact]
    public void Id_should_be_marked_with_the_Key_attribute()
    {
        var idProperty = typeof(Property<int, string>).GetProperty(nameof(Property<int, string>.Id))!;

        idProperty.GetCustomAttribute<KeyAttribute>().Should().NotBeNull();
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

    [Fact]
    public void Value_should_be_zero_on_a_newly_constructed_IntProperty()
    {
        var property = new IntProperty();

        property.Value.Should().Be(0);
    }

    [Fact]
    public void IntProperty_should_derive_from_the_two_type_parameter_Property()
    {
        var property = new IntProperty();

        property.Should().BeAssignableTo<Property<int, int>>();
    }

    [Fact]
    public void IntProperty_should_not_derive_from_the_single_type_parameter_Property_alias()
    {
        // Property<TValue> and IntProperty<TId> are parallel convenience aliases over
        // Property<TId, TValue>, not a single chain: IntProperty<TId> is generic over TId, so it
        // cannot inherit Property<TValue>, which fixes TId to int. The practical consequence is
        // that a repository or collection declared over Property<int> will not accept an
        // IntProperty, so the relationship is asserted rather than assumed.
        var property = new IntProperty();

        property.Should().NotBeAssignableTo<Property<int>>();
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

    [Fact]
    public void Value_should_be_null_on_a_newly_constructed_StringProperty()
    {
        var property = new StringProperty();

        property.Value.Should().BeNull();
    }

    [Fact]
    public void StringProperty_should_derive_from_the_two_type_parameter_Property()
    {
        var property = new StringProperty();

        property.Should().BeAssignableTo<Property<int, string>>();
    }
}
