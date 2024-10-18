using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore;

// TODO: Either remove this class or use it somewhere
public abstract class BaseDbContextFactory<TDbContext, TMigrationAssembly> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly Func<string> _connectionStringFunc;
    private readonly Func<DbContextOptions<TDbContext>, TDbContext> _dbContextCreator;
    private readonly Type? _migrationAssemblyType;

    protected BaseDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator) : this(dbContextCreator, ConnectionString.FromJsonFile())
    { }

    protected BaseDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator, Func<string> connectionStringFunc)
    {
        _dbContextCreator = dbContextCreator;
        _connectionStringFunc = connectionStringFunc;
    }

    public TDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();

        return _dbContextCreator(ConfigureOptions(_connectionStringFunc, optionsBuilder).Options);
    }

    protected static void ApplyMigrationsAssembly<TBuilder, TExtension>(RelationalDbContextOptionsBuilder<TBuilder, TExtension> builder)
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension> where TExtension : RelationalOptionsExtension, new()
    {
        var assembly = typeof(TMigrationAssembly).Assembly;
        Console.WriteLine($"Applying migrations assembly: {assembly}");
        builder.MigrationsAssembly(assembly.GetName().Name);
    }

    protected abstract DbContextOptionsBuilder<TDbContext> ConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TDbContext> optionsBuilder);
}