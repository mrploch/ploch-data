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
/// <typeparam name="TDbContext">The type of the <see cref="DbContext" /> being created.</typeparam>
/// <typeparam name="TMigrationAssembly">The type used to locate the assembly containing the EF Core migrations.</typeparam>
public abstract class BaseDbContextFactory<TDbContext, TMigrationAssembly> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly Func<string> _connectionStringFunc;
    private readonly Func<DbContextOptions<TDbContext>, TDbContext> _dbContextCreator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseDbContextFactory{TDbContext, TMigrationAssembly}" /> class.
    /// </summary>
    /// <param name="dbContextCreator">Function to create an instance of DbContext.</param>
    protected BaseDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator) : this(dbContextCreator, ConnectionString.FromJsonFile()!)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseDbContextFactory{TDbContext, TMigrationAssembly}" />
    ///     class.
    /// </summary>
    /// <param name="dbContextCreator">Function to create an instance of DbContext.</param>
    /// <param name="connectionStringFunc">Function to return the connection string.</param>
    protected BaseDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator, Func<string> connectionStringFunc)
    {
        _dbContextCreator = dbContextCreator;
        _connectionStringFunc = connectionStringFunc;
    }

    /// <summary>
    ///     Creates a new instance of the <typeparamref name="TDbContext" /> class.
    /// </summary>
    /// <param name="args">The arguments provided by the design-time tools.</param>
    /// <returns>A new instance of the <typeparamref name="TDbContext" /> class configured with the specified options.</returns>
    public TDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

        return _dbContextCreator(ConfigureOptions(_connectionStringFunc, optionsBuilder).Options);
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
        var assembly = typeof(TMigrationAssembly).Assembly;
        Console.WriteLine($"Applying migrations assembly: {assembly}");
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