namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>CreatedTime</c> property.
/// </summary>
/// <seealso cref="IHasCreatedBy" />
public interface IHasCreatedTime
{
    /// <summary>
    ///     The created time.
    /// </summary>
    /// <remarks>
    ///     The created time property is used to store the time when the entity was created.
    ///     It is commonly used with the <see cref="IHasCreatedBy" /> interface.
    /// </remarks>
    DateTimeOffset? CreatedTime { get; set; }
}