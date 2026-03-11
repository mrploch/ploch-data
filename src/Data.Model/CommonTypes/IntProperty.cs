namespace Ploch.Data.Model.CommonTypes;

/// <summary>
///     Represents a property with a <see cref="int" /> value.
/// </summary>
/// <typeparam name="TId">The type of the ID property.</typeparam>
public class IntProperty<TId> : Property<TId, int>
{ }

/// <summary>
///     Represents a property with an <see cref="int" /> Id and an <see cref="int" /> value.
/// </summary>
public class IntProperty : IntProperty<int>
{ }
