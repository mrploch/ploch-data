using Ploch.Data.Model.CommonTypes;

namespace Ploch.Data.Model;

/// <summary>
///     Represents an object that can have <see cref="Tag" /> entities.
/// </summary>
/// <typeparam name="TTag">The type of tags.</typeparam>
/// <typeparam name="TTagId">The type of tag IDs.</typeparam>
public interface IHasTags<TTag, TTagId> where TTag : Tag<TTagId>
{
    /// <summary>
    ///     Gets or sets the collection of tags.
    /// </summary>
    /// <returns>The collection of tags.</returns>
    /// <remarks>
    ///     The collection is annotated as non-nullable, but the interface cannot guarantee that an implementation
    ///     assigns it; an implementation that declares it without an initialiser holds <see langword="null" /> until the
    ///     caller assigns a collection or an object-relational mapper materialises the entity. See
    ///     <see cref="INamed.Name" /> for the same contract stated in full.
    /// </remarks>
    ICollection<TTag> Tags { get; set; }
}

/// <summary>
///     Represents an object that can have <see cref="Tag" /> entities.
/// </summary>
/// <typeparam name="TTag">The type of tags.</typeparam>
/// <remarks>
///     This interface inherits from <see cref="IHasTags{TTag,TId}" /> with a default id type of <see cref="int" />.
/// </remarks>
public interface IHasTags<TTag> : IHasTags<TTag, int> where TTag : Tag<int>
{
}
