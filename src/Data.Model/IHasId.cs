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
    ///         The interface cannot guarantee that an implementation assigns the identifier. In the common types
    ///         supplied by this library it is unset until the caller assigns it or the persistence layer generates it,
    ///         so a newly constructed entity carries <c>default(TId)</c> &#8212; <c>0</c> for an <see cref="int" />
    ///         identifier, <see cref="System.Guid.Empty" /> for a <see cref="System.Guid" /> one, and
    ///         <see langword="null" /> when <typeparamref name="TId" /> is a reference type such as
    ///         <see cref="string" />, despite the non-nullable annotation.
    ///     </para>
    ///     <para>
    ///         Where the generic types need a null-forgiving initialiser to reach that state &#8212; for example
    ///         <c>= default!</c> on <see cref="CommonTypes.Property{TId, TValue}" /> &#8212; it exists so that the
    ///         compiler accepts the Entity Framework Core materialisation path, on which the ORM populates the property
    ///         after construction. A closed value-type identifier such as <see cref="CommonTypes.Image" />'s
    ///         <see cref="int" /> needs no initialiser and reaches the same state implicitly. The supplied types declare
    ///         the property neither as <c>required</c> nor with a constructor or setter guard.
    ///     </para>
    /// </remarks>
    [Key]
    new TId Id { get; set; }
}
