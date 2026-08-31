namespace Ploch.Data.Model;

/// <summary>
///     An entity with a non-nullable <c>Value</c> property.
/// </summary>
/// <typeparam name="TValue">The type of the <c>Value</c> property.</typeparam>
public interface IHasValue<TValue>
{
    /// <summary>
    ///     The value property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The value is unset until the caller assigns it or an object-relational mapper materialises the entity. The
    ///         common types in this library initialise it with <c>= default!</c>, so a newly constructed entity carries
    ///         <c>default(TValue)</c> &#8212; <see langword="null" /> when <typeparamref name="TValue" /> is a reference
    ///         type, despite the non-nullable annotation.
    ///     </para>
    ///     <para>
    ///         The null-forgiving initialiser exists so that the compiler accepts the Entity Framework Core
    ///         materialisation path, on which the ORM populates the property after construction. The property is
    ///         deliberately neither <c>required</c> nor guarded at runtime.
    ///     </para>
    /// </remarks>
    TValue Value { get; set; }
}
