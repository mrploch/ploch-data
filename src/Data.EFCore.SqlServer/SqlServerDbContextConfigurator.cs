using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqlServer;

public class SqlServerDbContextConfigurator : IDbContextConfigurator
{
    private readonly string _connectionString;
    private readonly Action<SqlServerDbContextOptionsBuilder>? _optionsBuilderAction;

    public SqlServerDbContextConfigurator(Action<SqlConnectionStringBuilder> connectionStringBuilderAction,
                                          Action<SqlServerDbContextOptionsBuilder>? optionsBuilderAction =
                                              null) : this(GetConnectionString(connectionStringBuilderAction), optionsBuilderAction)
    { }

    public SqlServerDbContextConfigurator(string connectionString, Action<SqlServerDbContextOptionsBuilder>? optionsBuilderAction = null)
    {
        _connectionString = connectionString;
        _optionsBuilderAction = optionsBuilderAction;
    }

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