namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a non-nullable <c>Value</c> property.
/// </summary>
/// <typeparam name="TValue">The type of the <c>Value</c> property.</typeparam>
public interface IHasValue<TValue>
{
    /// <summary>
    ///     The value property.
    /// </summary>
    TValue Value { get; set; }
}