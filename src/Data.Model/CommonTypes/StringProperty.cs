namespace Ploch.Data.Model.CommonTypes;

/// <summary>
///     Represents a property with a string value.
/// </summary>
/// <typeparam name="TId">The type of the ID property.</typeparam>
public class StringProperty<TId> : Property<TId, string>
{ }

/// <summary>
///     Represents a property with an <see cref="int" /> Id and a <see cref="string" /> value.
/// </summary>
public class StringProperty : StringProperty<int>
{ }
