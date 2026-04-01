using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ploch.Data.EFCore.SqLite;

/// <summary>
///     An <see cref="IDbContextCreationLifecycle" /> implementation for SQLite that
///     applies the <c>DateTimeOffset</c> properties fix during model creation.
/// </summary>
/// <remarks>
///     <para>
///         SQLite does not natively support <see cref="DateTimeOffset" /> via Entity
///         Framework Core. This lifecycle implementation calls
///         <see cref="SqLiteDateTimeOffsetPropertiesFix.ApplySqLiteDateTimeOffsetPropertiesFix" />
///         during <see cref="OnModelCreating" /> to apply the necessary value converters
///         for all <see cref="DateTimeOffset" /> and <see cref="Nullable{DateTimeOffset}" />
///         properties in the model.
///     </para>
///     <para>
///         Register this implementation in the dependency injection container when the
///         application uses SQLite as its database provider:
///         <code>
///         services.AddSqLiteDbContextCreationLifecycle();
///         </code>
///     </para>
/// </remarks>
public class SqLiteDbContextCreationLifecycle : IDbContextCreationLifecycle
{
    /// <inheritdoc />
    /// <remarks>
    ///     Calls
    ///     <see cref="SqLiteDateTimeOffsetPropertiesFix.ApplySqLiteDateTimeOffsetPropertiesFix" />
    ///     to apply <c>DateTimeOffsetToBinaryConverter</c> to all <see cref="DateTimeOffset" />
    ///     properties when the provider is SQLite.
    /// </remarks>
    public void OnModelCreating(ModelBuilder modelBuilder, DatabaseFacade database)
    {
        modelBuilder.ApplySqLiteDateTimeOffsetPropertiesFix(database);
    }

    /// <inheritdoc />
    public void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }
}
