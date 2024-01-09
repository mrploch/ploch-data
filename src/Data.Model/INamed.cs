namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>Name</c> property.
/// </summary>
public interface INamed
{
    /// <summary>
    ///     The name of the entity.
    /// </summary>
    string Name { get; set; }
}