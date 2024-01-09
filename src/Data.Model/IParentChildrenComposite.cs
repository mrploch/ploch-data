namespace Ploch.Common.Data.Model;

/// <summary>
///     Represents an entity with the <c>Parent</c> and <c>Children</c>.
/// </summary>
/// <remarks>
///     <para>
///         Represents a hierarchical entity with the <c>Parent</c> property of type
///         <see cref="IHierarchicalWithParent{TParent}" /> and the
///         <c>Children</c> of type <see cref="IHierarchicalWithChildren{TChildren}" />.
///     </para>
///     <para>
///         See the <see cref="IHierarchicalWithParentComposite{TParent}" /> and
///         <see cref="IHierarchicalWithChildrenComposite{TChildren}" /> for more details.
///     </para>
/// </remarks>
/// <typeparam name="TParent">Type of the <c>Parent</c> property.</typeparam>
/// <typeparam name="TChildren">Type of the <c>Children</c> property.</typeparam>
public interface IParentChildrenComposite<TParent, TChildren> : IHierarchicalWithParentComposite<TParent>, IHierarchicalWithChildrenComposite<TChildren>
    where TParent : IHierarchicalWithParentComposite<TParent> where TChildren : IHierarchicalWithChildrenComposite<TChildren>
{ }