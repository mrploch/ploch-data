using Ploch.Common.Data.Model.CommonTypes;

namespace Ploch.Common.Data.Model;

/// <summary>
///     Describes a type that has a collection of <see cref="Category{TCategory}" />.
/// </summary>
/// <typeparam name="TCategory">The type of the <c>Categories</c>.</typeparam>
/// <typeparam name="TCategoryId">
///     Type type of the <typeparamref name="TCategory" /> <c>Id</c> property.
/// </typeparam>
public interface IHasCategories<TCategory, TCategoryId>
    where TCategory : Category<TCategory, TCategoryId>
{
    /// <summary>
    ///     A collection of categories which are a hierarchical parent / children structure.
    /// </summary>
    ICollection<TCategory>? Categories { get; set; }
}

/// <summary>
///     Describes a type that has a collection of <see cref="Category{TCategory}" /> where the <c>Category</c> <c>Id</c>
///     property
///     is <see cref="int" /> type.
/// </summary>
/// <inheritdoc cref="IHasCategories{TCategory,TCategoryId}" />
public interface IHasCategories<TCategory> : IHasCategories<TCategory, int>
    where TCategory : Category<TCategory, int>
{ }