using Microsoft.EntityFrameworkCore;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

public class SqLiteDbContextConfigurator : IDbContextConfigurator
{
    private readonly SqLiteConnectionOptions _options;

    public SqLiteDbContextConfigurator(SqLiteConnectionOptions options)
    {
        _options = options;
    }

    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        /*
         * var connection = new SqliteConnection(connectionString);
                                                       connection.Open();
                                                         builder.UseSqlite(connection);
         */
        var dbSource = _options.InMemory ? ":memory:" : _options.DbFilePath;
        optionsBuilder.UseSqlite($"Data Source={dbSource}");
    }
}