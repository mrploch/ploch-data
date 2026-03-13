using Ploch.Data.GenericRepository.Exceptions;

namespace Ploch.Data.GenericRepository.Tests;

public class DataAccessExceptionTests
{
    [Fact]
    public void Constructor_with_message_should_set_message()
    {
        var exception = new DataAccessException("test error");

        exception.Message.Should().Be("test error");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_with_message_and_inner_should_set_both()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new DataAccessException("outer", inner);

        exception.Message.Should().Be("outer");
        exception.InnerException.Should().BeSameAs(inner);
    }
}

public class DataReadExceptionTests
{
    [Fact]
    public void Constructor_with_message_should_set_message()
    {
        var exception = new DataReadException("read failed");

        exception.Message.Should().Be("read failed");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_with_message_and_inner_should_set_both()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new DataReadException("read failed", inner);

        exception.Message.Should().Be("read failed");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void DataReadException_should_inherit_from_DataAccessException()
    {
        var exception = new DataReadException("test");

        exception.Should().BeAssignableTo<DataAccessException>();
    }
}

public class DataUpdateExceptionTests
{
    [Fact]
    public void Constructor_with_message_should_set_message()
    {
        var exception = new DataUpdateException("update failed");

        exception.Message.Should().Be("update failed");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_with_message_and_inner_should_set_both()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new DataUpdateException("update failed", inner);

        exception.Message.Should().Be("update failed");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void DataUpdateException_should_inherit_from_DataAccessException()
    {
        var exception = new DataUpdateException("test");

        exception.Should().BeAssignableTo<DataAccessException>();
    }
}

public class DataUpdateConcurrencyExceptionTests
{
    [Fact]
    public void Constructor_with_message_should_set_message()
    {
        var exception = new DataUpdateConcurrencyException("concurrency");

        exception.Message.Should().Be("concurrency");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_with_message_and_inner_should_set_both()
    {
        var inner = new InvalidOperationException("inner");

        var exception = new DataUpdateConcurrencyException("concurrency", inner);

        exception.Message.Should().Be("concurrency");
        exception.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void DataUpdateConcurrencyException_should_inherit_from_DataUpdateException()
    {
        var exception = new DataUpdateConcurrencyException("test");

        exception.Should().BeAssignableTo<DataUpdateException>();
    }
}
