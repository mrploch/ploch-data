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
    /// <returns>An <see cref="IQueryable{T}" /> of the specified entity name.</returns>
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

        var entityType = FindEntityType(context, entityName);
        if (entityType == null)
        {
            throw new InvalidOperationException($"Entity type '{entityName}' was not found in the context.");
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

        if (!context.ContainsEntityType(entityType))
        {
            throw new InvalidOperationException($"Entity type '{entityType.Name}' was not found in the context.");
        }

        var setMethod = typeof(DbContext).GetMethods()
                                         .First(m => m.Name == "Set" && m.ContainsGenericParameters && m.GetParameters().Length == 0);

        var genericSetMethod = setMethod.MakeGenericMethod(entityType);

        return (IQueryable<object>)genericSetMethod.Invoke(context, null)!;
    }

    /// <summary>
    ///     Finds the entity type for the specified entity name in the given DbContext.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entityName">The name of the entity to find.</param>
    /// <returns>The <see cref="Type" /> of the specified entity name, or null if the entity type is not found.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="context" /> or <paramref name="entityName" /> is null.
    /// </exception>
    public static Type? FindEntityType(this DbContext context, string entityName)
    {
        return context.Model.GetEntityTypes().FirstOrDefault(t => t.ClrType.Name == entityName)?.ClrType;
    }

    /// <summary>
    ///     Determines whether the specified entity type is present in the DbContext.
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entityType">The Type of the entity to check for existence.</param>
    /// <returns>True if the entity type is present in the context; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="context" /> or <paramref name="entityType" /> is null.
    /// </exception>
    public static bool ContainsEntityType(this DbContext context, Type entityType)
    {
        return context.Model.GetEntityTypes().FirstOrDefault(t => t.ClrType == entityType) != null;
    }
}