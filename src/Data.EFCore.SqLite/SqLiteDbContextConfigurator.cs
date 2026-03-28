using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqLite;

/// <summary>
///     Configures a DbContext to use an SQLite database provider.
/// </summary>
/// <remarks>
///     <para>
///         For in-memory databases (<c>DataSource=:memory:</c>), this configurator creates and holds a single
///         <see cref="SqliteConnection" /> that is shared across all <see cref="DbContext" /> instances.
///         This is necessary because SQLite in-memory databases are destroyed when the connection is closed,
///         so sharing a connection ensures all DbContext instances access the same database.
///     </para>
///     <para>
///         Implements <see cref="IDisposable" /> to release the shared connection when testing is complete.
///     </para>
/// </remarks>
public class SqLiteDbContextConfigurator : IDbContextConfigurator, IDisposable
{
    private readonly Action<SqliteDbContextOptionsBuilder>? _dbContextOptionsAction;
    private readonly SqLiteConnectionOptions _options;
    private readonly object _connectionLock = new();
    private SqliteConnection? _sharedConnection;
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqLiteDbContextConfigurator" /> class.
    /// </summary>
    /// <param name="options">SqLite connection options. Defaults to <see cref="SqLiteConnectionOptions.InMemory" />.</param>
    /// <param name="dbContextOptionsAction">Action to execute on the SqLite DbContext options builder.</param>
    public SqLiteDbContextConfigurator(SqLiteConnectionOptions? options = null, Action<SqliteDbContextOptionsBuilder>? dbContextOptionsAction = null)
    {
        _options = options ?? SqLiteConnectionOptions.InMemory;
        _dbContextOptionsAction = dbContextOptionsAction;
    }

    /// <summary>
    ///     Configures the given DbContextOptionsBuilder to use SQLite as the database provider.
    /// </summary>
    /// <param name="optionsBuilder">The DbContextOptionsBuilder to configure.</param>
    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _options.BuildConnectionString();

        if (connectionString.Contains(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            // For in-memory databases, share a single connection across all DbContext instances.
            // Each new SQLite connection to :memory: creates a separate empty database, so sharing
            // ensures that schema and data are visible to all consumers (repositories, unit of work, etc.).
            lock (_connectionLock)
            {
                _sharedConnection ??= CreateAndOpenConnection(connectionString);
            }

            optionsBuilder.UseSqlite(_sharedConnection, _dbContextOptionsAction);
        }
        else
        {
            optionsBuilder.UseSqlite(connectionString, _dbContextOptionsAction);
        }
    }

    /// <summary>
    ///     Releases the shared SQLite connection, if one was created.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases the shared SQLite connection.
    /// </summary>
    /// <param name="disposing">Whether to release managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _sharedConnection?.Dispose();
            _sharedConnection = null;
        }

        _disposed = true;
    }

    private static SqliteConnection CreateAndOpenConnection(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        connection.Open();

        return connection;
    }
}
