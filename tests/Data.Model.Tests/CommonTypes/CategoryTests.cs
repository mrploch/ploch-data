using FluentAssertions;
using Ploch.Data.Model;
using Ploch.Data.Model.CommonTypes;
using Xunit;

namespace Ploch.Data.Model.Tests.CommonTypes;

public class CategoryTests
{
    [Fact]
    public void Id_should_be_settable_and_gettable()
    {
        var category = new TestCategory { Id = 42 };

        category.Id.Should().Be(42);
    }

    [Fact]
    public void Name_should_be_settable_and_gettable()
    {
        var category = new TestCategory { Name = "Electronics" };

        category.Name.Should().Be("Electronics");
    }

    [Fact]
    public void Parent_should_be_settable_and_gettable()
    {
        var parent = new TestCategory { Id = 1, Name = "Root" };
        var child = new TestCategory { Id = 2, Name = "Child", Parent = parent };

        child.Parent.Should().BeSameAs(parent);
    }

    [Fact]
    public void Children_should_be_settable_and_gettable()
    {
        var parent = new TestCategory { Id = 1, Name = "Root" };
        var child1 = new TestCategory { Id = 2, Name = "Child1" };
        var child2 = new TestCategory { Id = 3, Name = "Child2" };
        parent.Children = [child1, child2];

        parent.Children.Should().HaveCount(2);
        parent.Children.Should().Contain(child1);
        parent.Children.Should().Contain(child2);
    }

    [Fact]
    public void Category_should_implement_IHasId()
    {
        var category = new TestCategory();

        category.Should().BeAssignableTo<IHasId<int>>();
    }

    [Fact]
    public void Category_should_implement_INamed()
    {
        var category = new TestCategory();

        category.Should().BeAssignableTo<INamed>();
    }

    [Fact]
    public void Category_should_implement_IHierarchicalParentChildrenComposite()
    {
        var category = new TestCategory();

        category.Should().BeAssignableTo<IHierarchicalParentChildrenComposite<TestCategory>>();
    }

    [Fact]
    public void CategoryWithCustomId_should_support_guid_id()
    {
        var id = Guid.NewGuid();
        var category = new GuidCategory { Id = id, Name = "Test" };

        category.Id.Should().Be(id);
    }

    [Fact]
    public void Children_should_be_null_by_default()
    {
        var category = new TestCategory();

        category.Children.Should().BeNull();
    }

    [Fact]
    public void Parent_should_be_null_by_default()
    {
        var category = new TestCategory();

        category.Parent.Should().BeNull();
    }

    private sealed class TestCategory : Category<TestCategory>
    { }

    private sealed class GuidCategory : Category<GuidCategory, Guid>
    { }
}
