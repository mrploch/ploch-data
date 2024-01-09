using Ploch.Common.Data.Model.CommonTypes;

namespace Ploch.Common.Data.Model;

/// <summary>
///     Represents an entity with the <c>Parent</c> and <c>Children</c>.
/// </summary>
/// <remarks>
///     <para>
///         Represents a hierarchical entity with the <c>Parent</c> property of type
///         <see cref="IHierarchicalWithParentComposite{TParent}" /> and the
///         <c>Children</c> of type <see cref="IHierarchicalWithChildren{TChildren}" /> which have to be either this or
///         subtype of this type.
///     </para>
///     <para>
///         It allows building a data model with Parent and Children relationships.
///         An example type is the <see cref="Category{TCategory,TId}" />.
///     </para>
/// </remarks>
/// <typeparam name="TEntity">The type of the <c>Parent</c> and <c>Children</c>.</typeparam>
public interface IHierarchicalParentChildrenComposite<TEntity> : IParentChildrenComposite<TEntity, TEntity>
    where TEntity : IHierarchicalParentChildrenComposite<TEntity>
{ }