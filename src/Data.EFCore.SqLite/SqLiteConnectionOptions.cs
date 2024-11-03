using Microsoft.Data.Sqlite;

namespace Ploch.Data.EFCore.SqLite;

/// <summary>
///     Represents options for configuring an SQLite connection.
/// </summary>
public record SqLiteConnectionOptions
{
    private readonly SqliteConnectionStringBuilder _connectionStringBuilder = new();

    private SqLiteConnectionOptions(bool inMemory = false, string? dbFilePath = null)
    {
        if (inMemory && dbFilePath is not null)
        {
            throw new ArgumentException("Cannot specify both inMemory and dbFilePath");
        }

        var dbSource = inMemory ? ":memory:" : dbFilePath;
        if (!string.IsNullOrWhiteSpace(dbSource))
        {
            _connectionStringBuilder.DataSource = dbSource;
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqLiteConnectionOptions" /> class.
    /// </summary>
    /// <param name="connectionStringBuilderAction">Action to execute on <see cref="SqliteConnectionStringBuilder" />.</param>
    public SqLiteConnectionOptions(Action<SqliteConnectionStringBuilder> connectionStringBuilderAction)
    {
        connectionStringBuilderAction(_connectionStringBuilder);
    }

    /// <summary>
    ///     Provides a pre-configured option for creating an SQLite connection in memory.
    /// </summary>
    public static SqLiteConnectionOptions InMemory => new(true);

    /// <summary>
    ///     Builds and returns the SQLite connection string based on the configured options.
    /// </summary>
    /// <returns>The SQLite connection string.</returns>
    public string BuildConnectionString()
    {
        return _connectionStringBuilder.ToString();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqLiteConnectionOptions" /> class using the specified file path.
    /// </summary>
    /// <param name="dbFilePath">The file path to the SQLite database.</param>
    /// <returns>A new instance of <see cref="SqLiteConnectionOptions" /> configured to use the specified database file.</returns>
    public static SqLiteConnectionOptions UsingFile(string dbFilePath)
    {
        return new SqLiteConnectionOptions(dbFilePath: dbFilePath);
    }
}