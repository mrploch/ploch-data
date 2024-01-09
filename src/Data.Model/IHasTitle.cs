namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>Title</c> property.
/// </summary>
public interface IHasTitle
{
    /// <summary>
    ///     The title property.
    /// </summary>
    string Title { get; set; }
}