using System.ComponentModel.DataAnnotations;

namespace Ploch.Data.Model.CommonTypes;

/// <summary>
///     Represents a property with a Name and Value.
/// </summary>
/// <typeparam name="TId">The type of the ID property.</typeparam>
/// <typeparam name="TValue">The type of the Value property.</typeparam>
// CA1716: "Property" matches a keyword in Visual Basic. The name is central to the public API
// of this library and renaming it would be a breaking change for no practical benefit.
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Property<TId, TValue> : IHasId<TId>, INamed, IHasValue<TValue>
#pragma warning restore CA1716
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
// CA1716: see the note on Property<TId, TValue> above.
#pragma warning disable CA1716 // Identifiers should not match keywords
public class Property<TValue> : Property<int, TValue>
#pragma warning restore CA1716
{ }
