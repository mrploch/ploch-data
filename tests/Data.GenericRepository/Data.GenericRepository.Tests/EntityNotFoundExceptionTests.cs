using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.Tests;

public class EntityNotFoundExceptionTests
{
    [Fact]
    public void Constructor_should_set_entity_type_and_id()
    {
        var entityType = typeof(string);
        var id = 42;
        var message = "Not found";

        var exception = new EntityNotFoundException(entityType, id, message);

        exception.EntityType.Should().Be(entityType);
        exception.Id.Should().Be(id);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_with_inner_exception_should_set_all_properties()
    {
        var entityType = typeof(string);
        var id = "abc";
        var message = "Entity missing";
        var inner = new InvalidOperationException("inner");

        var exception = new EntityNotFoundException(entityType, id, message, inner);

        exception.EntityType.Should().Be(entityType);
        exception.Id.Should().Be(id);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void Create_should_produce_formatted_message()
    {
        var exception = EntityNotFoundException.Create<TestEntity, int>(99);

        exception.EntityType.Should().Be<TestEntity>();
        exception.Id.Should().Be(99);
        exception.Message.Should().Contain(nameof(TestEntity));
        exception.Message.Should().Contain("99");
    }

    private sealed class TestEntity : IHasId<int>
    {
        public int Id { get; set; }
    }
}
