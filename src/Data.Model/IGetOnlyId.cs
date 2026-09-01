using System.ComponentModel.DataAnnotations;

namespace Ploch.Data.Model;

/// <summary>
///     Defines a type that has an identifier which has get-only access.
/// </summary>
/// <remarks>
///     This type is not meant to represent an entity which has read only access. It is usually used in places where there
///     is no need to set the value.
/// </remarks>
/// <typeparam name="TId">The type of identifier.</typeparam>
public interface IGetOnlyId<out TId>
{
    /// <summary>
    ///     The identifier of the entity.
    /// </summary>
    /// <remarks>
    ///     The interface cannot guarantee that an implementation assigns the identifier; in the common types supplied by
    ///     this library it carries <c>default(TId)</c> until the caller assigns it or the persistence layer generates it.
    ///     See <see cref="IHasId{TId}.Id" /> for the full contract.
    /// </remarks>
    [Key]
    TId Id { get; }
}