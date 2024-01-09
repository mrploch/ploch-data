using Microsoft.EntityFrameworkCore;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

public interface IDbContextConfigurator
{
    void Configure(DbContextOptionsBuilder optionsBuilder);
}