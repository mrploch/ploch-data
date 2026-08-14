using Microsoft.EntityFrameworkCore.ChangeTracking;
using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore;

/// <summary>
///     Protects creation-audit properties from being overwritten when an update is applied via
///     <see cref="PropertyValues.SetValues(object)" />.
/// </summary>
internal static class CreationAuditPropertyProtector
{
    /// <summary>
    ///     Restores the persisted values of the creation-audit properties on the supplied entry.
    /// </summary>
    /// <param name="entry">The tracked entry the update was applied to.</param>
    /// <remarks>
    ///     SetValues copies every scalar from the supplied entity, so a partial detached entity
    ///     would overwrite the persisted creation-audit values with defaults. Creation audit is
    ///     write-once (set by <see cref="IAuditEntityHandler" /> on add), so the values loaded from
    ///     the database are restored and the properties are excluded from the update.
    /// </remarks>
    public static void RestoreCreationAuditValues(EntityEntry entry)
    {
        if (entry.Entity is IHasCreatedTime)
        {
            RestoreOriginalValue(entry, nameof(IHasCreatedTime.CreatedTime));
        }

        if (entry.Entity is IHasCreatedBy)
        {
            RestoreOriginalValue(entry, nameof(IHasCreatedBy.CreatedBy));
        }
    }

    private static void RestoreOriginalValue(EntityEntry entry, string propertyName)
    {
        if (entry.Metadata.FindProperty(propertyName) == null)
        {
            return;
        }

        var property = entry.Property(propertyName);
        property.CurrentValue = property.OriginalValue;
        property.IsModified = false;
    }
}
