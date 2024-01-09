using System.ComponentModel.DataAnnotations;

namespace Ploch.Common.Data.Model.CommonTypes;

/// <summary>
///     Represents a tag with an Id, Name, and Description.
/// </summary>
/// <typeparam name="TId">The type of the ID property.</typeparam>
public class Tag<TId> : IHasId<TId>, INamed, IHasDescription
{
    /// <inheritdoc cref="IHasId{TId}" />
    [Key]
    public TId Id { get; set; } = default!;

    /// <inheritdoc cref="INamed" />
    [MaxLength(128)]
    [Required]
    public string Name { get; set; } = null!;

    /// <inheritdoc cref="IHasDescription" />
    public string? Description { get; set; }
}

/// <summary>
///     RRepresents a tag with an Id, Name, and Description. The Id property is of <see cref="int" /> type.
/// </summary>
public class Tag : Tag<int>
{ }