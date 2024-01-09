using System.Collections.Generic;

namespace Ploch.Common.Data.Model;

/// <summary>
///     An entity with a <c>Children</c> property.
/// </summary>
/// <remarks>
///     An entity which is a composite of other entities, the <c>Children</c>.
/// </remarks>
/// <typeparam name="TChildren">The type of child entities.</typeparam>
public interface IHierarchicalWithChildren<TChildren>
{
    /// <summary>
    ///     The children of this entity.
    /// </summary>
    ICollection<TChildren>? Children { get; set; }
}