namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>Description</c> property.
/// </summary>
public interface IHasDescription
{
    /// <summary>
    ///     The description.
    /// </summary>
    string? Description { get; set; }
}