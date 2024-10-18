using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore;

public interface IDbContextConfigurator
{
    void Configure(DbContextOptionsBuilder optionsBuilder);
}