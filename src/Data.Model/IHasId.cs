using System.ComponentModel.DataAnnotations;

namespace Ploch.Data.Model;

/// <summary>
///     Defines a type that has an identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
public interface IHasId<TId> : IGetOnlyId<TId>
{
    /// <summary>
    ///     The identifier of the entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The identifier is unset until the caller assigns it or the persistence layer generates it. The common types
    ///         in this library initialise it with <c>= default!</c>, so a newly constructed entity carries
    ///         <c>default(TId)</c> &#8212; <c>0</c> for an <see cref="int" /> identifier, <see cref="Guid.Empty" /> for a
    ///         <see cref="Guid" /> one, and <see langword="null" /> when <typeparamref name="TId" /> is a reference type
    ///         such as <see cref="string" />, despite the non-nullable annotation.
    ///     </para>
    ///     <para>
    ///         The null-forgiving initialiser exists so that the compiler accepts the Entity Framework Core
    ///         materialisation path, on which the ORM populates the property after construction. The property is
    ///         deliberately neither <c>required</c> nor guarded at runtime.
    ///     </para>
    /// </remarks>
    [Key]
    new TId Id { get; set; }
}
