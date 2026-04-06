using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore;

/// <summary>
///     A no-op implementation of <see cref="IDbContextCreationLifecycle" /> that performs
///     no additional configuration during <see cref="DbContext" /> creation.
/// </summary>
/// <remarks>
///     Use this implementation for database providers that do not require any special
///     model-creation logic (e.g. SQL Server). It is registered automatically by
///     <c>AddDbContextWithRepositories&lt;TDbContext&gt;()</c> and related extension methods.
/// </remarks>
public class DefaultDbContextCreationLifecycle : IDbContextCreationLifecycle
{
    /// <inheritdoc />
    public void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database)
    {
        // No-op: the default lifecycle does not apply any provider-specific model configuration.
    }

    /// <inheritdoc />
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // No-op: the default lifecycle does not apply any provider-specific options configuration.
    }
}
