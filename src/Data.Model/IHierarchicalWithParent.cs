namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>Parent</c> property.
/// </summary>
/// <remarks>
///     An entity which has a reference to another entity, which is considered its parent.
/// </remarks>
/// <typeparam name="TParent">The type of the parent entity.</typeparam>
public interface IHierarchicalWithParent<TParent>
{
    /// <summary>
    ///     The parent of this entity.
    /// </summary>
    /// <summary>
    ///     The <c>Parent</c> property is used to store a reference to the parent entity.
    /// </summary>
    TParent? Parent { get; set; }
}