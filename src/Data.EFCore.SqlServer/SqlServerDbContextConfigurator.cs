using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqlServer;

/// <summary>
///     Configures a DbContext to use SQL Server as the database provider.
/// </summary>
public class SqlServerDbContextConfigurator : IDbContextConfigurator
{
    private readonly string _connectionString;
    private readonly Action<SqlServerDbContextOptionsBuilder>? _optionsBuilderAction;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerDbContextConfigurator" /> class.
    /// </summary>
    /// <param name="connectionStringBuilderAction">Configuration action to perform on connection string builder.</param>
    /// <param name="optionsBuilderAction">SqlServerDbContextOptionsBuilder configuration action.</param>
    public SqlServerDbContextConfigurator(Action<SqlConnectionStringBuilder> connectionStringBuilderAction,
                                          Action<SqlServerDbContextOptionsBuilder>? optionsBuilderAction =
                                              null) : this(GetConnectionString(connectionStringBuilderAction), optionsBuilderAction)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerDbContextConfigurator" /> class.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    /// <param name="optionsBuilderAction">Optional SqlServerDbContextOptionsBuilder configuration action.</param>
    public SqlServerDbContextConfigurator(string connectionString, Action<SqlServerDbContextOptionsBuilder>? optionsBuilderAction = null)
    {
        _connectionString = connectionString;
        _optionsBuilderAction = optionsBuilderAction;
    }

    /// <summary>
    ///     Configures the specified DbContext with SQL Server options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
        _optionsBuilderAction?.Invoke(new SqlServerDbContextOptionsBuilder(optionsBuilder));
    }

    private static string GetConnectionString(Action<SqlConnectionStringBuilder> connectionStringBuilderAction)
    {
        var builder = new SqlConnectionStringBuilder();
        connectionStringBuilderAction(builder);

        return builder.ToString();
    }
}