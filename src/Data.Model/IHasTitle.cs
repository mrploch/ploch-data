namespace Ploch.Data.Model;

/// <summary>
///     An entity with a <c>Title</c> property.
/// </summary>
public interface IHasTitle : IHasTitleReadOnly
{
    /// <summary>
    ///     The title property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The property is annotated as non-nullable, but the interface cannot guarantee that an implementation
    ///         assigns it. No common type supplied by this library implements <see cref="IHasTitle" />, so the statement
    ///         is about the expectations placed on implementers rather than about supplied behaviour: an implementation
    ///         that declares the property as <c>public string Title { get; set; } = null!;</c> &#8212; the shape used
    ///         throughout this repository's documentation and the shape the supplied types use for
    ///         <see cref="INamed.Name" /> &#8212; holds <see langword="null" /> until the caller assigns a title or an
    ///         object-relational mapper materialises the entity. That initialiser exists so that the compiler accepts
    ///         the Entity Framework Core materialisation path, on which the ORM populates the property after
    ///         construction.
    ///     </para>
    ///     <para>
    ///         Implementations are free to be stricter &#8212; declaring the property <c>required</c>, initialising it
    ///         in a constructor, or validating it &#8212; so code written against the interface should not rely on
    ///         either behaviour. Assigning a title before the entity is used or persisted is the caller's
    ///         responsibility. See <see cref="INamed.Name" /> for the same contract stated against the supplied common
    ///         types.
    ///     </para>
    /// </remarks>
    new string Title { get; set; }
}

/// <summary>
///     Represents an entity that provides read-only access to a <c>Title</c> property.
/// </summary>
public interface IHasTitleReadOnly
{
    /// <summary>
    ///     The title property.
    /// </summary>
    /// <remarks>
    ///     The property is annotated as non-nullable, but the interface cannot guarantee that an implementation assigns
    ///     it; it is commonly <see langword="null" /> until a title is assigned or an object-relational mapper
    ///     materialises the entity. See <see cref="IHasTitle.Title" /> for the full contract.
    /// </remarks>
    string Title { get; }
}
