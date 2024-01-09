namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>CreatedBy</c> property.
/// </summary>
/// <remarks>
///     An entity with a <c>CreatedBy</c> property which is a name or identifier of the user who created the entity.
/// </remarks>
/// <seealso cref="IHasCreatedTime" />
public interface IHasCreatedBy
{
    /// <summary>
    ///     The created by property.
    /// </summary>
    /// <remarks>
    ///     The created by property. It can be used to track the user who created the entity.
    ///     <br />
    ///     It is commonly used with the <see cref="IHasCreatedTime" /> interface.
    /// </remarks>
    string? CreatedBy { get; set; }
}