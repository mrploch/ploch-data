using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqlServer;

/// <summary>
///     Configures a DbContext to use SQL Server as the database provider.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="SqlServerDbContextConfigurator" /> class.
/// </remarks>
/// <param name="connectionString">The connection string to use.</param>
/// <param name="optionsBuilderAction">Optional SqlServerDbContextOptionsBuilder configuration action.</param>
public class SqlServerDbContextConfigurator(string connectionString, Action<SqlServerDbContextOptionsBuilder>? optionsBuilderAction = null)
    : IDbContextConfigurator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerDbContextConfigurator" /> class.
    /// </summary>
    /// <param name="connectionStringBuilderAction">Configuration action to perform on connection string builder.</param>
    /// <param name="optionsBuilderAction">SqlServerDbContextOptionsBuilder configuration action.</param>
    public SqlServerDbContextConfigurator(Action<SqlConnectionStringBuilder> connectionStringBuilderAction,
#pragma warning disable SA1003 // line breaks
                                          Action<SqlServerDbContextOptionsBuilder>? optionsBuilderAction = null) :
#pragma warning restore SA1003
        this(GetConnectionString(connectionStringBuilderAction), optionsBuilderAction)
    { }

    /// <summary>
    ///     Configures the specified DbContext with SQL Server options.
    /// </summary>
    /// <param name="optionsBuilder">The option builder to configure.</param>
    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder = optionsBuilder.UseSqlServer(connectionString);
        optionsBuilderAction?.Invoke(new(optionsBuilder));
    }

    private static string GetConnectionString(Action<SqlConnectionStringBuilder> connectionStringBuilderAction)
    {
        var builder = new SqlConnectionStringBuilder();
        connectionStringBuilderAction(builder);

        return builder.ToString();
    }
}
