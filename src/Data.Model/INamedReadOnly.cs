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
    string Name { get; }
}
