using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore.SqLite;

public abstract class SqLiteDbContextFactory<TDbContext, TMigrationAssembly> : BaseDbContextFactory<TDbContext, TMigrationAssembly>
    where TDbContext : DbContext
{
    protected SqLiteDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator) : base(dbContextCreator)
    { }

    protected SqLiteDbContextFactory(Func<DbContextOptions<TDbContext>, TDbContext> dbContextCreator, Func<string> connectionStringFunc) : base(dbContextCreator,
        connectionStringFunc)
    { }

    protected override DbContextOptionsBuilder<TDbContext> ConfigureOptions(Func<string> connectionStringFunc, DbContextOptionsBuilder<TDbContext> optionsBuilder)
    {
        return optionsBuilder.UseSqlite(connectionStringFunc(), ApplyMigrationsAssembly);
    }
}