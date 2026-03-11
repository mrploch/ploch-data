using System.ComponentModel.DataAnnotations;

namespace Ploch.Data.Model.CommonTypes;

/// <summary>
///     Represents a property with a Name and Value.
/// </summary>
/// <typeparam name="TId">The type of the ID property.</typeparam>
/// <typeparam name="TValue">The type of the Value property.</typeparam>
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Property<TId, TValue> : IHasId<TId>, INamed, IHasValue<TValue>
{
    /// <inheritdoc cref="IHasId{TId}.Id" />
    [Key]
    public TId Id { get; set; } = default!;

    /// <inheritdoc cref="INamed.Name" />
    public string Name { get; set; } = null!;

    /// <inheritdoc cref="IHasValue{TValue}.Value" />
    public TValue Value { get; set; } = default!;
}

/// <summary>
///     Represents a property with an <see cref="int" /> Id.
/// </summary>
/// <typeparam name="TValue">The type of the Value property.</typeparam>
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Property<TValue> : Property<int, TValue>
#pragma warning restore CA1716
{ }
