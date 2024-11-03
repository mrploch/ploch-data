using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqlServer;

/// <summary>
///     Represents a factory for creating instances of TDbContext configured to use SQL Server as the database provider.
///     This abstract class provides methods to configure the DbContext using a connection string and optional additional
///     SQL Server-specific configuration.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext to be created.</typeparam>
/// <typeparam name="TMigrationAssembly">The type used to locate the migration assemblies.</typeparam>
public abstract class SqlServerDbContextFactory<TDbContext, TMigrationAssembly> : BaseDbContextFactory<TDbContext, TMigrationAssembly>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerDbContextFactory{TDbContext, TMigrationAssembly}" /> class.
    /// </summary>
    /// <param name="dbContextCreator">Function to create an instance of DbContext.</param>
    protected SqlServerDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator) : base(dbContextCreator)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlServerDbContextFactory{TDbContext, TMigrationAssembly}" />
    ///     class.
    /// </summary>
    /// <param name="dbContextCreator">Function to create an instance of DbContext.</param>
    /// <param name="connectionStringFunc">Function to return the connection string.</param>
    protected SqlServerDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator, Func<string> connectionStringFunc) : base(dbContextCreator,
                                                                                                                                                   connectionStringFunc)
    { }

    /// <summary>
    ///     Configures the options for the DbContext using the provided connection string function.
    /// </summary>
    /// <param name="connectionStringFunc">Function to retrieve the database connection string.</param>
    /// <param name="optionsBuilder">The options builder to be configured.</param>
    /// <returns>The configured DbContextOptionsBuilder.</returns>
    protected override DbContextOptionsBuilder<TDbContext> ConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TDbContext> optionsBuilder)
    {
        ConfigureDbContextAction(connectionStringFunc, ApplyMigrationsAssembly);

        return optionsBuilder;
    }

    // This is left for future use.
    // ReSharper disable once UnusedMethodReturnValue.Local
#pragma warning disable S3241
    private static Action<DbContextOptionsBuilder> ConfigureDbContextAction(Func<string> connectionStringFunc,
#pragma warning restore S3241
                                                                            Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
    {
        return optionsBuilder => optionsBuilder.UseSqlServer(connectionStringFunc(), sqlServerOptionsAction);
    }
}