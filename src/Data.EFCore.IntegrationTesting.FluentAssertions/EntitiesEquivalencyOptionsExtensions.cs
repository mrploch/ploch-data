using FluentAssertions;
using FluentAssertions.Equivalency;

namespace Ploch.Data.EFCore.IntegrationTesting;

/// <summary>
///     Provides FluentAssertions equivalency extension methods for comparing EF Core entities
///     stored and retrieved from a database.
/// </summary>
public static class EntitiesEquivalencyOptionsExtensions
{
    /// <summary>
    ///     Configures FluentAssertions equivalency options suitable for comparing EF Core entities
    ///     that have been stored in and retrieved from a database.
    /// </summary>
    /// <typeparam name="TSelf">The concrete type of the equivalency options, used for the fluent chain.</typeparam>
    /// <param name="options">The equivalency options to configure.</param>
    /// <returns>
    ///     The same <paramref name="options" /> instance with the entity-comparison settings applied,
    ///     allowing further chaining.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         Three recurring issues arise when comparing in-memory entity objects with entities loaded from a
    ///         relational database. This method handles all three in a single call:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <b>Collection ordering:</b> Databases do not guarantee the order in which rows are
    ///                 returned. <see cref="SelfReferenceEquivalencyOptions{TSelf}.WithoutStrictOrdering" />
    ///                 ensures collection items are matched by value, not position.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <b>Cyclic navigation properties:</b> EF Core entity graphs commonly form reference
    ///                 cycles — for example <c>BlogPost → Tag → BlogPosts → BlogPost</c>. Without handling,
    ///                 FluentAssertions recurses indefinitely.
    ///                 <see cref="EquivalencyOptionsExtensions.IgnoringCyclicReferences{TSelf}" /> stops the
    ///                 traversal when a cycle is detected.
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <b><see cref="DateTimeOffset" /> precision:</b> SQLite stores
    ///                 <see cref="DateTimeOffset" /> values as TEXT with approximately 100-microsecond
    ///                 precision, while .NET retains 100-nanosecond (tick) precision. The maximum observed
    ///                 difference is ~78 µs. A <b>1-millisecond tolerance</b> (10× the maximum rounding
    ///                 error) is applied to every <see cref="DateTimeOffset" /> property comparison via
    ///                 <see cref="FluentAssertions.Primitives.DateTimeOffsetAssertions.BeCloseTo" />.
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         When EF Core loads an entity with eager-loaded navigation properties, it also populates the
    ///         inverse back-navigation references (e.g. <c>Tag.BlogPosts</c>). In-memory entities created in
    ///         test setup do not have those back-references. Exclude them from the comparison and verify them
    ///         separately if needed.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    ///     // Basic comparison of two entities loaded from the database.
    ///     actual.Should().BeEquivalentTo(expected, options => options.WithEntityEquivalencyOptions());
    ///
    ///     // Combined with exclusions for back-navigation properties that differ between an in-memory
    ///     // object and a DB-loaded one (e.g. Tag.BlogPosts is populated by EF Core but not in test setup).
    ///     actual.Should().BeEquivalentTo(expected,
    ///         options => options.Excluding(p => p.Tags)
    ///                           .Excluding(p => p.Categories)
    ///                           .WithEntityEquivalencyOptions());
    ///
    ///     // Collection assertion — ContainEquivalentOf and ContainEquivalentOf both accept the same options.
    ///     blogPosts.Should().ContainEquivalentOf(expected,
    ///         options => options.Excluding(p => p.Categories).WithEntityEquivalencyOptions());
    ///     </code>
    /// </example>
    public static TSelf WithEntityEquivalencyOptions<TSelf>(this SelfReferenceEquivalencyOptions<TSelf> options)
        where TSelf : SelfReferenceEquivalencyOptions<TSelf>
    {
        // 1ms tolerance is ~10× the maximum observed precision loss when SQLite truncates
        // sub-microsecond ticks from a stored DateTimeOffset value.
        return options.WithoutStrictOrdering()
                      .IgnoringCyclicReferences()
                      .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromMilliseconds(1)))
                      .WhenTypeIs<DateTimeOffset>();
    }
}
