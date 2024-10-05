using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqlServer;

public class SqlServerDbContextConfigurator : IDbContextConfigurator
{
    private readonly Action<SqlServerDbContextOptionsBuilder> _optionsBuilderAction;

    public SqlServerDbContextConfigurator(Action<SqlServerDbContextOptionsBuilder> optionsBuilderAction)
    {
        _optionsBuilderAction = optionsBuilderAction;
    }

    public void Configure(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=Lists;Trusted_Connection=True;");
    }
}