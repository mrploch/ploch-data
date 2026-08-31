using System.ComponentModel.DataAnnotations;

namespace Ploch.Data.Model.CommonTypes;

/// <summary>
///     Typical category model class with the <c>Id</c> property of <see cref="int" /> type.
/// </summary>
/// <remarks>
///     <para>
///         Typical category model class that can be used as a base type in data models. It has a nested structure with a
///         parent and children.
///     </para>
///     <para>
///         This type can be extended with additional properties in the application data model by inheriting from it.
///     </para>
/// </remarks>
/// <typeparam name="TCategory">The actual category type in the data model.</typeparam>
public class Category<TCategory> : Category<TCategory, int>
    where TCategory : Category<TCategory, int>
{ }

/// <summary>
///     Typical category model class.
/// </summary>
/// <remarks>
///     <para>
///         Typical category model class that can be used as a base type in data models. It has a nested structure with a
///         parent and children.
///     </para>
///     <para>
///         This type can be extended with additional properties in the application data model by inheriting from it.
///     </para>
/// </remarks>
/// <typeparam name="TCategory">The actual category type in the data model.</typeparam>
/// <typeparam name="TId">The type of the <c>Id</c> property.</typeparam>
public class Category<TCategory, TId> : IHasId<TId>, INamed, IHierarchicalParentChildrenComposite<TCategory>
    where TCategory : Category<TCategory, TId>
{
    /// <inheritdoc cref="IHasId{TId}.Id" />
    [Key]
    public TId Id { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the collection of child categories.
    /// </summary>
    /// <value>
    ///     The collection of child categories.
    /// </value>
    public virtual ICollection<TCategory>? Children { get; set; }

    /// <summary>
    ///     The parent category.
    /// </summary>
    public virtual TCategory? Parent { get; set; }

    /// <summary>
    ///     The name of the category.
    /// </summary>
    /// <remarks>
    ///     Although annotated as non-nullable, the name is <see langword="null" /> until the caller assigns it or an
    ///     object-relational mapper materialises the entity. See <see cref="INamed.Name" /> for the full contract.
    /// </remarks>
    [MaxLength(128)]
    public string Name { get; set; } = default!;
}