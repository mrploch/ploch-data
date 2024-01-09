using System.ComponentModel.DataAnnotations;

namespace Ploch.Common.Data.Model.CommonTypes;

/// <summary>
///     Entity representing an image.
/// </summary>
public class Image : IHasId<int>, INamed, IHasDescription
{
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly - conflicts with the nullability.
    /// <summary>
    ///     The image binary contents.
    /// </summary>
    public byte[]? Contents { get; set; }
#pragma warning restore SA1011

    /// <inheritdoc cref="IHasDescription.Description" />
    [MaxLength(512)]
    public string? Description { get; set; }

    /// <inheritdoc cref="IHasId{TId}.Id" />
    [Key]
    public int Id { get; set; }

    /// <inheritdoc cref="INamed.Name" />
    [MaxLength(255)]
    public string Name { get; set; } = default!;
}