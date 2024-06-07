using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;

/// <summary>
///     SqLite model builder extension to apply a workaround for DateTimeOffset properties
/// </summary>
public static class SqLiteDateTimeOffsetPropertiesFix
{
    /// <summary>
    ///     Applies the SqLite DateTimeOffset properties fix to the model builder allowing usage of DateTimeOffset properties
    ///     in SQLite databases.
    /// </summary>
    /// <remarks>
    ///     SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
    ///     here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
    ///     To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
    ///     use the DateTimeOffsetToBinaryConverter
    ///     Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
    ///     This only supports millisecond precision, but should be sufficient for most use cases.
    /// </remarks>
    /// <param name="builder">The model builder.</param>
    /// <param name="database">The database.</param>
    /// <returns>The model builder for further usage.</returns>
    public static ModelBuilder ApplySqLiteDateTimeOffsetPropertiesFix(this ModelBuilder builder, DatabaseFacade database)
    {
        if (database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
            // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
            // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
            // use the DateTimeOffsetToBinaryConverter
            // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
            // This only supports millisecond precision, but should be sufficient for most use cases.
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    builder.Entity(entityType.Name).Property(property.Name).HasConversion(new DateTimeOffsetToBinaryConverter());
                }
            }
        }

        return builder;
    }
}