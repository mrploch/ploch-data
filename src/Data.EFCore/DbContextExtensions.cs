using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Ploch.Data.EFCore;

/// <summary>
///     Provides extension methods for the <see cref="DbContext" /> class.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    ///     Retrieves an entity set dynamically for the specified entity name.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entityName">The name of the entity set to retrieve.</param>
    /// <returns>An <see cref="IQueryable" /> of the specified entity name.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="context" /> or <paramref name="entityName" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the entity set for the specified <paramref name="entityName" /> cannot be retrieved.
    /// </exception>
    public static IQueryable<object> Set(this DbContext context, string entityName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityName);

        var entityType = context.Model.GetEntityTypes().FirstOrDefault(t => t.ClrType.Name == entityName)?.ClrType;
        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity type '{entityName}' not found in the context.");
        }

        return context.Set(entityType);
    }

    /// <summary>
    ///     Retrieves an entity set dynamically for the specified entity type.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entityType">The type of entity to retrieve.</param>
    /// <returns>An <see cref="IQueryable{T}" /> of the specified entity type.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="context" /> or <paramref name="entityType" /> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the DbSet for the specified <paramref name="entityType" /> cannot be retrieved.
    /// </exception>
    public static IQueryable<object> Set(this DbContext context, Type entityType)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityType);

        return (IQueryable<object>)context.GetType().GetMethod("Set")!.MakeGenericMethod(entityType).Invoke(context, null)! ??
               throw new InvalidOperationException("Failed to get DbSet for type.");
    }
}