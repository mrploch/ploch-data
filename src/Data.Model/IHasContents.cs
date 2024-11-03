namespace Ploch.Data.Model;

/// <summary>
///     Represents an entity that has textual contents.
/// </summary>
public interface IHasContents
{
    /// <summary>
    ///     Gets or sets the textual contents associated with the entity.
    /// </summary>
    public string? Contents { get; set; }
}