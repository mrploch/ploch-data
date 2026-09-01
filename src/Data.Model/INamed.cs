namespace Ploch.Data.Model;

/// <summary>
///     An entity with a <c>Name</c> property.
/// </summary>
public interface INamed : INamedReadOnly
{
    /// <summary>
    ///     Gets or sets a name of the entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The property is annotated as non-nullable, but the interface cannot guarantee that an implementation
    ///         assigns it. The common types supplied by this library &#8212; <see cref="CommonTypes.Property{TId, TValue}" />,
    ///         <see cref="CommonTypes.Tag{TId}" />, <see cref="CommonTypes.Category{TCategory, TId}" /> and
    ///         <see cref="CommonTypes.Image" /> &#8212; initialise it with a null-forgiving initialiser, so a freshly
    ///         constructed instance of one of them holds <see langword="null" /> until the caller assigns a name or an
    ///         object-relational mapper materialises the entity. That initialiser exists so that the compiler accepts
    ///         the Entity Framework Core materialisation path, on which the ORM populates the property after
    ///         construction.
    ///     </para>
    ///     <para>
    ///         Those types declare the property neither as <c>required</c> nor with a constructor or setter guard, so a
    ///         name is not enforced at construction time and a deliberate null-forgiving assignment can set the property
    ///         back to <see langword="null" />. Validation metadata is a separate concern from assignment behaviour:
    ///         <see cref="CommonTypes.Tag{TId}" />, alone among the supplied types, annotates the property with
    ///         <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute" />, which constrains validation and
    ///         the generated database column rather than in-memory assignment &#8212; and with nullable reference types
    ///         enabled Entity Framework Core already maps a non-nullable <c>string Name</c> to a <c>NOT NULL</c> column,
    ///         so the attribute affects <c>DataAnnotations</c> validation more than the generated schema. Assigning a
    ///         name before the entity is used or persisted is therefore the caller's responsibility.
    ///     </para>
    ///     <para>
    ///         Implementations outside this library are free to be stricter &#8212; declaring the property
    ///         <c>required</c>, initialising it in a constructor, or validating it &#8212; so code written against the
    ///         interface should not rely on the permissive behaviour of the supplied common types.
    ///     </para>
    /// </remarks>
    new string Name { get; set; }
}
