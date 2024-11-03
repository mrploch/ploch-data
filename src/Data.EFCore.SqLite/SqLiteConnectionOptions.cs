using Microsoft.Data.Sqlite;

namespace Ploch.Data.EFCore.SqLite;

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

    public SqLiteConnectionOptions(Action<SqliteConnectionStringBuilder> connectionStringBuilderAction)
    {
        connectionStringBuilderAction(_connectionStringBuilder);
    }

    public static SqLiteConnectionOptions InMemory => new(true);

    public string BuildConnectionString()
    {
        return _connectionStringBuilder.ToString();
    }

    public static SqLiteConnectionOptions UsingFile(string dbFilePath)
    {
        return new SqLiteConnectionOptions(dbFilePath: dbFilePath);
    }
}