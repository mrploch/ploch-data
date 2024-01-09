namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>LastModifiedBy</c> property.
/// </summary>
/// <remarks>
///     An entity with a <c>LastModifiedBy</c> property which is used to track the user who last modified the entity.
/// </remarks>
/// <seealso cref="IHasModifiedTime" />
public interface IHasModifiedBy
{
    /// <summary>
    ///     The last modified by property.
    /// </summary>
    /// <remarks>
    ///     The last modified by property is used to track the user who last modified the entity.
    ///     It is commonly used with the <see cref="IHasModifiedTime" /> interface.
    /// </remarks>
    string? LastModifiedBy { get; set; }
}