namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with the <c>LastAccessedBy</c> property.
/// </summary>
/// <remarks>
///     An entity with the <c>LastAccessedBy</c> property which is used to track the user who last accessed the entity.
/// </remarks>
/// <seealso cref="IHasAccessedTime" />
public interface IHasAccessedBy
{
    /// <summary>
    ///     The last accessed by property.
    /// </summary>
    /// <remarks>
    ///     The last accessed by property is used to track the user who last accessed the entity.
    ///     It is commonly used with the <see cref="IHasAccessedTime" /> interface.
    /// </remarks>
    string? LastAccessedBy { get; set; }
}