using System.ComponentModel.DataAnnotations;

namespace Ploch.Common.Data.Model;

/// <summary>
///     Defines an entity that has an <c>Id</c> property that has a setter method.
/// </summary>
/// <typeparam name="TId">The type of the <c>Id</c> property.</typeparam>
/// <inheritdoc />
public interface IHasIdSettable<TId> : IHasId<TId>
{
    /// <inheritdoc cref="IHasId{TId}.Id" />
    /// />
    [Key]
    new TId Id { get; set; }
}