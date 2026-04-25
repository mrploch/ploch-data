using FluentAssertions;
using Ploch.Data.Model;
using Ploch.Data.Model.CommonTypes;
using Xunit;

namespace Ploch.Data.Model.Tests.CommonTypes;

public class TagTests
{
    [Fact]
    public void Id_should_be_settable_and_gettable()
    {
        var tag = new Tag { Id = 5 };

        tag.Id.Should().Be(5);
    }

    [Fact]
    public void Name_should_be_settable_and_gettable()
    {
        var tag = new Tag { Name = "important" };

        tag.Name.Should().Be("important");
    }

    [Fact]
    public void Description_should_be_settable_and_gettable()
    {
        var tag = new Tag { Description = "This is important" };

        tag.Description.Should().Be("This is important");
    }

    [Fact]
    public void Description_should_be_nullable()
    {
        var tag = new Tag { Name = "test" };

        tag.Description.Should().BeNull();
    }

    [Fact]
    public void Tag_should_implement_IHasId()
    {
        var tag = new Tag();

        tag.Should().BeAssignableTo<IHasId<int>>();
    }

    [Fact]
    public void Tag_should_implement_INamed()
    {
        var tag = new Tag();

        tag.Should().BeAssignableTo<INamed>();
    }

    [Fact]
    public void Tag_should_implement_IHasDescription()
    {
        var tag = new Tag();

        tag.Should().BeAssignableTo<IHasDescription>();
    }

    [Fact]
    public void TagWithCustomId_should_support_guid_id()
    {
        var id = Guid.NewGuid();
        var tag = new Tag<Guid> { Id = id, Name = "guid-tag" };

        tag.Id.Should().Be(id);
        tag.Name.Should().Be("guid-tag");
    }
}
