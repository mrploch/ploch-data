using System.Collections;
using FluentAssertions.Equivalency;

namespace Ploch.Data.EFCore.IntegrationTesting.FluentAssertions;

/// <summary>
///     An <see cref="IEquivalencyStep" /> that treats a <see langword="null" /> collection
///     as equivalent to an empty collection (and vice versa).
/// </summary>
/// <remarks>
///     <para>
///         EF Core does not initialise navigation collections that were not eager-loaded via
///         <c>Include()</c> — they remain <see langword="null" />. In-memory test entities,
///         however, typically initialise collections to <c>new List&lt;T&gt;()</c>. Without
///         this step, FluentAssertions treats <see langword="null" /> and an empty collection
///         as different, causing false-negative assertion failures.
///     </para>
///     <para>
///         This step only intercedes when one side is <see langword="null" /> and the other is
///         an empty <see cref="IEnumerable" /> (excluding <see cref="string" />, which also
///         implements <see cref="IEnumerable" />). All other cases are passed through to the
///         next step in the pipeline, preserving configured options such as
///         <see cref="DateTimeOffset" /> tolerance and cyclic-reference handling.
///     </para>
/// </remarks>
internal sealed class NullEmptyCollectionEquivalencyStep : IEquivalencyStep
{
    /// <inheritdoc />
    public EquivalencyResult Handle(Comparands comparands, IEquivalencyValidationContext context, IValidateChildNodeEquivalency valueChildNodes)
    {
        if (comparands.Subject is null && IsEmptyNonStringEnumerable(comparands.Expectation))
        {
            return EquivalencyResult.EquivalencyProven;
        }

        if (comparands.Expectation is null && IsEmptyNonStringEnumerable(comparands.Subject))
        {
            return EquivalencyResult.EquivalencyProven;
        }

        return EquivalencyResult.ContinueWithNext;
    }

    private static bool IsEmptyNonStringEnumerable(object? value)
    {
        if (value is string or null)
        {
            return false;
        }

        if (value is IEnumerable enumerable)
        {
            var enumerator = enumerable.GetEnumerator();
            try
            {
                return !enumerator.MoveNext();
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }

        return false;
    }
}
