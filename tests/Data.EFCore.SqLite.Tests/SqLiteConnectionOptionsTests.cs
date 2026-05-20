using FluentAssertions;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Ploch.Data.EFCore.SqLite.Tests;

public class SqLiteConnectionOptionsTests
{
    [Fact]
    public void InMemory_should_return_options_with_memory_datasource()
    {
        var options = SqLiteConnectionOptions.InMemory;
        var connectionString = options.BuildConnectionString();

        connectionString.Should().Contain("Data Source=:memory:");
    }

    [Fact]
    public void UsingFile_should_return_options_with_specified_datasource()
    {
        var dbPath = "test.db";
        var options = SqLiteConnectionOptions.UsingFile(dbPath);
        var connectionString = options.BuildConnectionString();

        connectionString.Should().Contain($"Data Source={dbPath}");
    }

    [Fact]
    public void FromConnectionString_should_return_options_with_specified_connection_string()
    {
        var connectionString = "Data Source=test_cs.db;Mode=ReadOnly";
        var options = SqLiteConnectionOptions.FromConnectionString(connectionString);

        // Microsoft.Data.Sqlite.SqliteConnectionStringBuilder.ToString() does not preserve
        // original keyword casing/order, so assert semantic equivalence via its own parser
        // rather than string equality against the input.
        var builtFromOptions = new SqliteConnectionStringBuilder(options.BuildConnectionString());
        var expectedBuilder = new SqliteConnectionStringBuilder(connectionString);

        builtFromOptions.DataSource.Should().Be(expectedBuilder.DataSource);
        builtFromOptions.Mode.Should().Be(expectedBuilder.Mode);
    }

    [Fact]
    public void Constructor_with_action_should_apply_action_to_builder()
    {
        var options = new SqLiteConnectionOptions(builder =>
                                                  {
                                                      builder.DataSource = "custom.db";
                                                      builder.Mode = SqliteOpenMode.ReadWriteCreate;
                                                  });

        var connectionString = options.BuildConnectionString();
        connectionString.Should().Contain("Data Source=custom.db");
        connectionString.Should().Contain("Mode=ReadWriteCreate");
    }

    [Fact]
    public void BuildConnectionString_should_return_consistent_string()
    {
        var options = SqLiteConnectionOptions.InMemory;
        var cs1 = options.BuildConnectionString();
        var cs2 = options.BuildConnectionString();

        cs1.Should().Be(cs2);
    }
}
