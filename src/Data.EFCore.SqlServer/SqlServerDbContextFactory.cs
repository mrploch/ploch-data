using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqlServer;

public abstract class SqlServerDbContextFactory<TDbContext, TMigrationAssembly> : BaseDbContextFactory<TDbContext, TMigrationAssembly>
    where TDbContext : DbContext
{
    protected SqlServerDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator) : base(dbContextCreator)
    { }

    protected SqlServerDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator, Func<string> connectionStringFunc) :
        base(dbContextCreator,
             connectionStringFunc)
    { }

    public static Action<DbContextOptionsBuilder> ConfigureDbContextAction(Func<string> connectionStringFunc,
                                                                           Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
    {
        return optionsBuilder => optionsBuilder.UseSqlServer(connectionStringFunc(), sqlServerOptionsAction);
    }

    protected override DbContextOptionsBuilder<TDbContext> ConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TDbContext> optionsBuilder)
    {
        ConfigureDbContextAction(connectionStringFunc, ApplyMigrationsAssembly);

        return optionsBuilder;
    }
}