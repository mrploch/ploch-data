namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>TParent</c> property which have to be of type or a subtype o this entity.
/// </summary>
/// <remarks>
///     An entity which has a reference to another entity, which is considered its parent.
///     The parent have to be of the type or a subtype of this entity.
///     <para>
///         It is commonly used to represent a tree structure where the parent is of the same type as the parent.
///         <br />
///         When used with the <see cref="IHierarchicalWithChildrenComposite{T}" /> interface, it can be used to represent
///         a tree structure where the children have
///         a reference to the parent.
///     </para>
///     <para>
///         Example used of this structure is a common builder or an element of a tree view in the UI.
///     </para>
/// </remarks>
/// <typeparam name="TParent">The type of the parent entity.</typeparam>
public interface IHierarchicalWithParentComposite<TParent> : IHierarchicalWithParent<TParent>
    where TParent : IHierarchicalWithParent<TParent>
{ }