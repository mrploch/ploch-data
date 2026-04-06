using FluentAssertions;
using Ploch.Data.Model;
using Ploch.Data.Model.CommonTypes;
using Xunit;

namespace Ploch.Data.Model.Tests.CommonTypes;

public class ImageTests
{
    [Fact]
    public void Id_should_be_settable_and_gettable()
    {
        var image = new Image { Id = 1 };

        image.Id.Should().Be(1);
    }

    [Fact]
    public void Name_should_be_settable_and_gettable()
    {
        var image = new Image { Name = "logo.png" };

        image.Name.Should().Be("logo.png");
    }

    [Fact]
    public void Description_should_be_settable_and_gettable()
    {
        var image = new Image { Description = "Company logo" };

        image.Description.Should().Be("Company logo");
    }

    [Fact]
    public void Description_should_be_nullable()
    {
        var image = new Image { Name = "test.png" };

        image.Description.Should().BeNull();
    }

    [Fact]
    public void Contents_should_be_settable_and_gettable()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        var image = new Image { Contents = bytes };

        image.Contents.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void Contents_should_be_nullable()
    {
        var image = new Image { Name = "empty.png" };

        image.Contents.Should().BeNull();
    }

    [Fact]
    public void Image_should_implement_IHasId()
    {
        var image = new Image();

        image.Should().BeAssignableTo<IHasId<int>>();
    }

    [Fact]
    public void Image_should_implement_INamed()
    {
        var image = new Image();

        image.Should().BeAssignableTo<INamed>();
    }

    [Fact]
    public void Image_should_implement_IHasDescription()
    {
        var image = new Image();

        image.Should().BeAssignableTo<IHasDescription>();
    }
}
