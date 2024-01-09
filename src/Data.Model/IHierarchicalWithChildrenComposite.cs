namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>Children</c> property which have to be of type or a subtype o this entity.
/// </summary>
/// <remarks>
///     An entity which is a composite of other entities, the <c>Children</c>.
///     The children have to be of the type or a subtype of this entity.
///     <para>
///         It is commonly used to represent a tree structure where the children are of the same type as the parent.
///         <br />
///         When used with the <see cref="IHierarchicalWithParentComposite{T}" /> interface, it can be used to represent a
///         tree structure where the children have
///         a reference to the parent.
///     </para>
///     <para>
///         Example used of this structure is a common builder or an element of a tree view in the UI.
///     </para>
/// </remarks>
/// <typeparam name="TChildren">The type of child entities.</typeparam>
public interface IHierarchicalWithChildrenComposite<TChildren> : IHierarchicalWithChildren<TChildren>
    where TChildren : IHierarchicalWithChildren<TChildren>
{ }