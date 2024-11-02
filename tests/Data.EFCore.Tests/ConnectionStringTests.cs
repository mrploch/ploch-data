using FluentAssertions;

namespace Ploch.Data.EFCore.Tests;

public class ConnectionStringTests
{
    [Fact]
    public void FromJsonFile_should_load_connection_string_from_appsettings()
    {
        var connectionString = ConnectionString.FromJsonFile();
        connectionString.Should().NotBeNull();
        connectionString().Should().Be("Server=localhost;Database=DB1;Integrated Security=True;TrustServerCertificate=True");
    }

    [Fact]
    public void FromJsonFile_should_load_connection_string_from_specified_configuration_file()
    {
        var connectionString = ConnectionString.FromJsonFile("appsettings2.json");
        connectionString.Should().NotBeNull();
        connectionString().Should().Be("Server=localhost;Database=DB2;Integrated Security=True;TrustServerCertificate=True");
    }
}