using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Common.Data.EFCore;

public abstract class BaseDbContextFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly Func<string> _connectionStringFunc;
    private readonly Func<DbContextOptions<TDbContext>, TDbContext> _dbContextCreator;

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

    protected void ApplyMigrationsAssembly<TBuilder, TExtension>(RelationalDbContextOptionsBuilder<TBuilder, TExtension> builder)
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension> where TExtension : RelationalOptionsExtension, new()
    {
        Console.WriteLine("Applying migrations assembly: " + GetType().Assembly.GetName().Name);
        builder.MigrationsAssembly(GetType().Assembly.GetName().Name);
    }

    protected abstract DbContextOptionsBuilder<TDbContext> ConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TDbContext> optionsBuilder);
}