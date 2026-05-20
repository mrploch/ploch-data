using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore;

/// <summary>
///     Defines a pluggable lifecycle hook for <see cref="DbContext" /> creation.
/// </summary>
/// <remarks>
///     <para>
///         Implementations of this interface allow database-specific logic to be injected
///         into a <see cref="DbContext" /> without the <see cref="DbContext" /> itself
///         needing to reference any specific database provider.
///     </para>
///     <para>
///         A typical use case is applying the SQLite <c>DateTimeOffset</c> properties fix
///         (via <c>ApplySqLiteDateTimeOffsetPropertiesFix</c>) only when the application
///         is configured to use SQLite, while keeping the core Data project
///         database-agnostic.
///     </para>
///     <para>
///         Register the appropriate implementation via dependency injection.
///         Use <see cref="DefaultDbContextCreationLifecycle" /> for providers that
///         require no special model creation logic (e.g. SQL Server), and a
///         provider-specific implementation (e.g. <c>SqLiteDbContextCreationLifecycle</c>)
///         for providers that need customisation.
///     </para>
/// </remarks>
/// <example>
///     Inject into a <see cref="DbContext" /> constructor and call the lifecycle methods
///     from the <c>OnModelCreating</c> and <c>OnConfiguring</c> overrides:
///     <code>
///     public class MyDbContext : DbContext
///     {
///         private readonly IDbContextCreationLifecycle _lifecycle;
///
///         public MyDbContext(DbContextOptions&lt;MyDbContext&gt; options,
///                           IDbContextCreationLifecycle lifecycle)
///             : base(options)
///             => _lifecycle = lifecycle;
///
///         protected override void OnModelCreating(ModelBuilder modelBuilder)
///         {
///             modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
///             _lifecycle.OnModelCreating(modelBuilder, Database);
///             base.OnModelCreating(modelBuilder);
///         }
///
///         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
///         {
///             base.OnConfiguring(optionsBuilder);
///             _lifecycle.OnConfiguring(optionsBuilder);
///         }
///     }
///     </code>
/// </example>
public interface IDbContextCreationLifecycle
{
    /// <summary>
    ///     Called during <see cref="DbContext.OnModelCreating" /> to apply
    ///     provider-specific model configuration.
    /// </summary>
    /// <param name="modelBuilder">The model builder being configured.</param>
    /// <param name="database">
    ///     The <see cref="DatabaseFacade" /> for the current context, which can be
    ///     used to inspect the database provider name.
    /// </param>
    void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database);

    /// <summary>
    ///     Called during <see cref="DbContext.OnConfiguring" /> to apply
    ///     provider-specific options configuration.
    /// </summary>
    /// <param name="optionsBuilder">The options builder being configured.</param>
    void OnConfiguring(DbContextOptionsBuilder optionsBuilder);
}
