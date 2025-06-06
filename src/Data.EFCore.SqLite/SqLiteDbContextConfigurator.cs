using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqLite;

/// <summary>
///     Configures a DbContext to use an SQLite database provider.
/// </summary>
public class SqLiteDbContextConfigurator : IDbContextConfigurator
{
    private readonly Action<SqliteDbContextOptionsBuilder>? _dbContextOptionsAction;
    private readonly SqLiteConnectionOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqLiteDbContextConfigurator" /> class.
    /// </summary>
    /// <param name="options">SqLite connection options.</param>
    /// <param name="dbContextOptionsAction">Action to execute on the SqLite DbContext options builder.</param>
    public SqLiteDbContextConfigurator(SqLiteConnectionOptions? options = null, Action<SqliteDbContextOptionsBuilder>? dbContextOptionsAction = null)
    {
        _dbContextOptionsAction = dbContextOptionsAction;

        _options = options ?? SqLiteConnectionOptions.InMemory;
    }

    /// <summary>
    ///     Configures the given DbContextOptionsBuilder to use SQLite as the database provider.
    /// </summary>
    /// <param name="optionsBuilder">The DbContextOptionsBuilder to configure.</param>
    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_options.BuildConnectionString(), _dbContextOptionsAction);
    }
}
