namespace Ploch.Common.Data.Model;

/// <summary>
///     Defines a type that has a parent identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
/// <seealso cref="IHasId{TId}" />
/// <seealso cref="IHierarchicalWithParent{T}" />
public interface IHasParentId<out TId>
{
    /// <summary>
    ///     The parent identifier.
    /// </summary>
    TId ParentId { get; }
}