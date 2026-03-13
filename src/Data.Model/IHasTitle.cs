namespace Ploch.Data.Model;

/// <summary>
///     An entity with a <c>Title</c> property.
/// </summary>
public interface IHasTitle : IHasTitleReadOnly
{
    /// <summary>
    ///     The title property.
    /// </summary>
    new string Title { get; set; }
}

/// <summary>
///     Represents an entity that provides read-only access to a <c>Title</c> property.
/// </summary>
public interface IHasTitleReadOnly
{
    /// <summary>
    ///     The title property.
    /// </summary>
    string Title { get; }
}
