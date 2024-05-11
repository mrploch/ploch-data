using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

public class SqLiteDbContextConfigurator : IDbContextConfigurator
{
    private readonly Action<SqliteDbContextOptionsBuilder>? _dbContextOptionsAction;
    private readonly SqLiteConnectionOptions _options;

    public SqLiteDbContextConfigurator(SqLiteConnectionOptions? options = null, Action<SqliteDbContextOptionsBuilder>? dbContextOptionsAction = null)
    {
        _dbContextOptionsAction = dbContextOptionsAction;

        _options = options ?? SqLiteConnectionOptions.InMemory;
    }

    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_options.BuildConnectionString(), _dbContextOptionsAction);
    }
}