using FluentAssertions;
using Ploch.Data.EFCore.IntegrationTesting;

namespace Ploch.Data.EFCore.Tests;

public class DbContextExtensionsTests : DataIntegrationTest<TestDbContext>
{
    [Fact]
    public void Set_should_return_DbSet_for_a_given_name()
    {
        DbContext.Set(nameof(AnotherTestEntity)).Should().BeSameAs(DbContext.AnotherTestEntities);
    }

    [Fact]
    public void Set_should_return_DbSet_for_a_given_entity_type()
    {
        DbContext.Set(typeof(AnotherTestEntity)).Should().BeSameAs(DbContext.AnotherTestEntities);
    }

    [Fact]
    public void Set_should_throw_InvalidOperationException_if_provided_entity_name_is_not_found_in_the_model()
    {
        var act = () => DbContext.Set(nameof(NonExistingEntity));

        act.Should().Throw<InvalidOperationException>().WithMessage("*'NonExistingEntity'*not found*");
    }

    [Fact]
    public void Set_should_throw_InvalidOperationException_if_provided_entity_type_is_not_found_in_the_model()
    {
        var act = () => DbContext.Set(typeof(NonExistingEntity));

        act.Should().Throw<InvalidOperationException>().WithMessage("*'NonExistingEntity'*not found*");
    }

    [Fact]
    public void FindEntityType_should_find_requested_type_in_db_context()
    {
        DbContext.FindEntityType(nameof(TestEntity)).Should().NotBeNull().And.Be(typeof(TestEntity));
    }

    [Fact]
    public void FindEntityType_should_return_null_if_type_is_not_part_of_db_context()
    {
        DbContext.FindEntityType(nameof(NonExistingEntity)).Should().BeNull();
    }

    [Fact]
    public void ContainsEntityType_should_return_true_if_provided_type_is_part_of_db_context()
    {
        DbContext.ContainsEntityType(typeof(TestEntity)).Should().BeTrue();
    }

    [Fact]
    public void ContainsEntityType_should_return_false_if_provided_type_is_not_part_of_db_context()
    {
        DbContext.ContainsEntityType(typeof(NonExistingEntity)).Should().BeFalse();
    }

#pragma warning disable S2094 - we need to test the exception
    private class NonExistingEntity
#pragma warning restore S2094
    { }
}