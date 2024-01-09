using System;

namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>AccessedTime</c> property.
/// </summary>
/// <remarks>
///     An entity with a <c>AccessedTime</c> property used to store the last time when an entity was accessed.
/// </remarks>
/// <seealso cref="IHasAccessedBy" />
public interface IHasAccessedTime
{
    /// <summary>
    ///     The accessed time.
    /// </summary>
    /// <remarks>
    ///     The accessed time property is used to store the time when the entity was last accessed.
    ///     It is commonly used with the <see cref="IHasAccessedBy" /> interface.
    /// </remarks>
    DateTimeOffset? AccessedTime { get; set; }
}