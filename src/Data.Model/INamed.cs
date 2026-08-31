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
    ///         Although the property is annotated as non-nullable, it is <see langword="null" /> until the caller assigns
    ///         it or an object-relational mapper materialises the entity. The common types in this library initialise it
    ///         with the null-forgiving operator (<c>= null!</c>) so that the compiler accepts the Entity Framework Core
    ///         materialisation path, on which the ORM populates the property after construction.
    ///     </para>
    ///     <para>
    ///         The property is deliberately neither <c>required</c> nor guarded at runtime, so a name is not enforced at
    ///         construction time and a deliberate null-forgiving assignment can set it back to <see langword="null" />.
    ///         Assigning a name before the entity is used or persisted is therefore the caller's responsibility.
    ///     </para>
    /// </remarks>
    new string Name { get; set; }
}
