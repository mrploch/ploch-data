using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore;

/// <summary>
///     A no-op implementation of <see cref="IDbContextCreationLifecycle" /> that performs
///     no additional configuration during <see cref="DbContext" /> creation.
/// </summary>
/// <remarks>
///     Use this implementation for database providers that do not require any special
///     model-creation logic (e.g. SQL Server). It is the default registered by
///     <see cref="DbContextCreationLifecycleServiceCollectionExtensions.AddDefaultDbContextCreationLifecycle" />.
/// </remarks>
public class DefaultDbContextCreationLifecycle : IDbContextCreationLifecycle
{
    /// <inheritdoc />
    public void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database)
    {
    }

    /// <inheritdoc />
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}
