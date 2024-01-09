namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>IHasModifiedTime</c> property.
/// </summary>
/// <remarks>
///     An entity with a <c>IHasModifiedTime</c> property which is used to store the time of the last modification of the
///     entity.
/// </remarks>
/// <seealso cref="IHasModifiedTime" />
public interface IHasModifiedTime
{
    /// <summary>
    ///     The last modified time property.
    /// </summary>
    /// <remarks>
    ///     The last modified time property is used to store the time of the last modification of the entity.
    ///     It is commonly used with the <see cref="IHasModifiedBy" /> interface.
    /// </remarks>
    DateTimeOffset? ModifiedTime { get; set; }
}