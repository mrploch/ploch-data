namespace Ploch.Data.Model;

/// <summary>
///     Represents an entity that contains a property for storing notes.
/// </summary>
public interface IHasNotes
{
    /// <summary>
    ///     Gets or sets the notes associated with the entity.
    /// </summary>
    /// <value>
    ///     A string containing the notes, or <c>null</c> if no notes are set.
    /// </value>
    string? Notes { get; set; }
}
