namespace Ploch.Data.Model;

/// <summary>
///     Represents an entity with a read-only <c>Name</c> property.
/// </summary>
public interface INamedReadOnly
{
    /// <summary>
    ///     Gets the name of the entity.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the entity.
    /// </value>
    /// <remarks>
    ///     The property is annotated as non-nullable, but the interface cannot guarantee that an implementation assigns
    ///     it; in the common types supplied by this library it is <see langword="null" /> until a name is assigned or an
    ///     object-relational mapper materialises the entity. See <see cref="INamed.Name" /> for the full contract.
    /// </remarks>
    string Name { get; }
}
