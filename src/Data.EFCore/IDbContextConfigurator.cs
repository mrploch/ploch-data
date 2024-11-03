using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore;

/// <summary>
///     Defines the contract for configuring a DbContext.
/// </summary>
public interface IDbContextConfigurator
{
    /// <summary>
    ///     Configures the specified DbContext with DB server options.
    /// </summary>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    void Configure(DbContextOptionsBuilder optionsBuilder);
}