using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore;

/// <summary>
///     Provides a base factory for creating instances of <see cref="DbContext" /> at design-time.
///     This abstract class is designed to be inherited by specific factory implementations for different database
///     providers.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext" /> this factory is created for.</typeparam>
/// <typeparam name="TFactory">
///     The type of <see cref="BaseDbContextFactory{TDbContext,TFactory}" /> you are implementing,
///     see remarks for details.
/// </typeparam>
/// <remarks>
///     <para>
///         Provides a base implementation of <see cref="IDesignTimeDbContextFactory{TDbContext}" /> used to create
///         <see cref="DbContext" /> instances at design-time.
///     </para>
///     <para>
///         It also configures the migrations assembly to be the one where the <typeparamref name="TFactory" /> type is defined.
///     </para>
///     <para>
///         This type would usually be implemented in an assembly that contains the migrations and would configure DbContext to
///         use a specific database. It would configure the provider and specify other options, like a connection string.
///         The <typeparamref name="TFactory" /> type parameter is used to determine the assembly that contains the migrations.
///     </para>
///     <para>
///         This type makes it easy to use a different assembly for migrations than the one that contains the DbContext.
///         One of the scenarios where this is useful is when targeting multiple databases using the same DbContext.
///         You can take a look at the <see href="https://github.com/mrploch/ploch-data/tree/main/samples/SampleApp">SampleApp</see> project
///         for an example of how to use this class.
///     </para>
/// </remarks>
/// <param name="dbContextCreator">Function to create an instance of DbContext.</param>
/// <param name="connectionStringFunc">Function to return the connection string.</param>
public abstract class BaseDbContextFactory<TDbContext, TFactory>(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator, Func<string> connectionStringFunc)
    : IDesignTimeDbContextFactory<TDbContext> where TDbContext : DbContext where TFactory : BaseDbContextFactory<TDbContext, TFactory>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseDbContextFactory{TDbContext, TMigrationAssembly}" /> class.
    /// </summary>
    /// <param name="dbContextCreator">Function to create an instance of DbContext.</param>
    protected BaseDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator) : this(dbContextCreator, ConnectionString.FromJsonFile()!)
    { }

    /// <summary>
    ///     Creates a new instance of the <typeparamref name="TDbContext" /> class.
    /// </summary>
    /// <param name="args">The arguments provided by the design-time tools.</param>
    /// <returns>A new instance of the <typeparamref name="TDbContext" /> class configured with the specified options.</returns>
    public TDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

        return dbContextCreator(ConfigureOptions(connectionStringFunc, optionsBuilder).Options);
    }

    /// <summary>
    ///     Configures the migration assembly for the supplied <paramref name="builder" />.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the builder.</typeparam>
    /// <typeparam name="TExtension">The type of the extension.</typeparam>
    /// <param name="builder">The relational database context options builder.</param>
    protected static void ApplyMigrationsAssembly<TBuilder, TExtension>(RelationalDbContextOptionsBuilder<TBuilder, TExtension> builder)
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension> where TExtension : RelationalOptionsExtension, new()
    {
        var assembly = typeof(TFactory).Assembly;

        builder.MigrationsAssembly(assembly.GetName().Name);
    }

    /// <summary>
    ///     Configures the DbContext options using the specified connection string function and options builder.
    /// </summary>
    /// <param name="connectionStringFunc">A function that provides the connection string.</param>
    /// <param name="optionsBuilder">An options builder for configuring the DbContext options.</param>
    /// <returns>The configured DbContextOptionsBuilder.</returns>
    protected abstract DbContextOptionsBuilder<TDbContext> ConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TDbContext> optionsBuilder);
}
