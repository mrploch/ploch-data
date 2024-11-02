using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore.SqLite;

/// <summary>
///     Provides a factory for creating DbContext instances with SQLite as the database provider.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
/// <typeparam name="TMigrationAssembly">The type of the migration assembly.</typeparam>
/// <remarks>
///     This abstract class extends BaseDbContextFactory and provides methods to configure SQLite options
///     for the DbContext instances it creates.
/// </remarks>
public abstract class SqLiteDbContextFactory<TDbContext, TMigrationAssembly> : BaseDbContextFactory<TDbContext, TMigrationAssembly>
    where TDbContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqLiteDbContextFactory{TDbContext, TMigrationAssembly}" /> class.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Provides a factory for creating DbContext instances with SQLite as the database provider.
    ///     </para>
    ///     <para>
    ///         This abstract class extends BaseDbContextFactory and provides methods to configure SQLite options
    ///         for the DbContext instances it creates.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
    /// <typeparam name="TMigrationAssembly">The type of the migration assembly.</typeparam>
    /// <param name="dbContextCreator">Function that creates an instance of <typeparamref name="TDbContext" />.</param>
    protected SqLiteDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator) : base(dbContextCreator)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqLiteDbContextFactory{TDbContext, TMigrationAssembly}" /> class.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Provides a factory for creating DbContext instances with SQLite as the database provider.
    ///     </para>
    ///     <para>
    ///         This abstract class extends BaseDbContextFactory and provides methods to configure SQLite options
    ///         for the DbContext instances it creates.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
    /// <typeparam name="TMigrationAssembly">The type of the migration assembly.</typeparam>
    /// <param name="dbContextCreator">A function that creates an instance of <typeparamref name="TDbContext" />.</param>
    /// <param name="connectionStringFunc">A function that returns the connection string.</param>
    protected SqLiteDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator, Func<string> connectionStringFunc) : base(dbContextCreator,
                                                                                                                                                connectionStringFunc)
    { }

    /// <summary>
    ///     Configures the options for the DbContext instance.
    /// </summary>
    /// <param name="connectionStringFunc">A function that returns the connection string.</param>
    /// <param name="optionsBuilder">The DbContextOptionsBuilder used to configure the context.</param>
    /// <returns>The configured DbContextOptionsBuilder instance.</returns>
    protected override DbContextOptionsBuilder<TDbContext> ConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TDbContext> optionsBuilder)
    {
        return optionsBuilder.UseSqlite(connectionStringFunc(), ApplyMigrationsAssembly);
    }
}