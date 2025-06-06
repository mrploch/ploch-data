namespace Ploch.Data.Model;

/// <summary>
///     An entity with a <c>Name</c> property.
/// </summary>
public interface INamed : INamedReadOnly
{
    /// <summary>
    ///     Gets or sets a name of the entity.
    /// </summary>
    new string Name { get; set; }
}
