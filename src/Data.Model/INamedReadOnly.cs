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
    ///     Although the property is annotated as non-nullable, it is <see langword="null" /> until the entity is given a
    ///     name, either by the caller or by an object-relational mapper materialising it. See
    ///     <see cref="INamed.Name" /> for the full contract.
    /// </remarks>
    string Name { get; }
}
