﻿using System.ComponentModel.DataAnnotations;

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
    [Key]
    new TId Id { get; set; }
}