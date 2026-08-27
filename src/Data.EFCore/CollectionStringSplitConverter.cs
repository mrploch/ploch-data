using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ploch.Data.EFCore;

/// <summary>
///     Converts a collection of values to a delimited string and vice versa.
/// </summary>
/// <remarks>
///     <para>
///         Values are serialised and deserialised using <see cref="CultureInfo.InvariantCulture" />,
///         so round-trips are stable regardless of the current thread culture.
///     </para>
///     <para>
///         Each element is written as its invariant-culture string, escaped with
///         <see cref="Uri.EscapeDataString(string)" />, and the escaped elements are joined with the
///         separator. An element produces an <i>empty</i> segment only when it is
///         <see langword="null" /> or its invariant representation is empty — among the supported
///         element types that means <see langword="null" /> or <see cref="string.Empty" />. An empty
///         segment always reads back as <c>default(TValue)</c>.
///     </para>
///     <para>
///         A <see langword="null" /> <i>collection</i> is distinct from an empty one: it is written
///         as <see langword="null" /> (a <c>NULL</c> column) and read back as <see langword="null" />,
///         whereas an empty collection is written as the empty string and read back as an empty
///         collection.
///     </para>
///     <para>
///         For value-typed elements the encoding is therefore <i>cardinality-preserving</i>: every
///         non-<see langword="null" /> value writes at least one character, so the writer never
///         emits an empty segment and an empty payload means exactly an empty collection. This is
///         what makes a single-element collection such as <c>[0]</c> or <c>[false]</c> round-trip;
///         earlier versions stored any element equal to <c>default(TValue)</c> as an empty segment
///         and silently reloaded such a collection as empty. Cardinality is preserved, which is not
///         the same as every <i>value</i> surviving intact — see the <see cref="DateTime" />
///         limitation below.
///     </para>
///     <para>
///         <b>The separator must contain at least one character that cannot occur in escaped
///         element data</b>, and this is enforced by the constructor. Escaped data is drawn from
///         exactly two sources: the RFC 3986 <i>unreserved</i> characters — <c>A-Z</c>, <c>a-z</c>,
///         <c>0-9</c>, <c>-</c>, <c>.</c>, <c>_</c>, <c>~</c> — emitted literally, and
///         percent-triplets, which introduce <c>%</c>. A separator built only from those characters
///         can appear inside an element and would tear it apart on read: with <c>separator: "-"</c>
///         the element <c>"a-b"</c> would be written as <c>a-b</c>, and with <c>separator: "%2C"</c>
///         an element containing a comma would be written as <c>a%2Cb</c> — the separator's own
///         spelling. The default <c>","</c> is safe, as is any separator containing a reserved
///         character such as <c>;</c> or <c>|</c>.
///     </para>
///     <para>
///         Known limitations, tracked for a future format revision:
///         a <see cref="string" /> element that is empty is indistinguishable from
///         <see langword="null" /> and reads back as <see langword="null" />; a collection holding a
///         single empty or <see langword="null" /> string is indistinguishable from an empty
///         collection and reads back empty; and <see cref="DateTime" /> elements lose sub-second
///         precision and <see cref="DateTimeKind" />, because the invariant general format has
///         neither a fractional-seconds field nor an offset.
///     </para>
///     <para>
///         <typeparamref name="TValue" /> must be convertible <i>from a <see cref="string" /></i> by
///         <see cref="Convert.ChangeType(object, Type, IFormatProvider)" /> — that is the
///         <see cref="IConvertible" /> types, which includes <see cref="decimal" /> and
///         <see cref="DateTime" /> as well as <see cref="string" /> and the numeric and
///         <see cref="bool" /> primitives. Other types (for example <see cref="Guid" />, enums or
///         <see cref="Nullable{T}" />) serialise but throw <see cref="InvalidCastException" /> when
///         read back.
///     </para>
/// </remarks>
/// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
public class CollectionStringSplitConverter<TValue> : ValueConverter<ICollection<TValue>, string>
{
#pragma warning disable EF1001
    /// <summary>
    ///     Initializes a new instance of the <see cref="CollectionStringSplitConverter{TValue}" /> class.
    /// </summary>
    /// <remarks>
    ///     Converts a collection of values to a delimited string and vice versa.
    /// </remarks>
    /// <param name="separator">
    ///     Separator to be used when converting the collection to string. It must contain at least
    ///     one character that <see cref="Uri.EscapeDataString(string)" /> escapes, or an element
    ///     containing the separator could not be distinguished from a delimiter on read — see the
    ///     remarks on <see cref="CollectionStringSplitConverter{TValue}" />. The default <c>","</c>
    ///     is safe.
    /// </param>
    /// <param name="convertNulls">Include null values in the conversion.</param>
    /// <param name="mappingHints">Optional mapping hints to pass to the base converter.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="separator" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="separator" /> is empty, or consists only of characters that
    ///     <see cref="Uri.EscapeDataString(string)" /> leaves unescaped.
    /// </exception>
#pragma warning disable SA1003 // Symbols should be spaced correctly - : should not appear at the end of the line - line is too long
    public CollectionStringSplitConverter(string separator = ",", bool convertNulls = true, ConverterMappingHints? mappingHints = null) :
#pragma warning restore SA1003
        base(values => Serialize(values, separator)!,
             s => Deserialize(s, separator)!,
             convertNulls,
             mappingHints)
#pragma warning restore EF1001
    {
        // Runs after the base constructor, which only stores the expressions — neither is invoked
        // until a conversion happens, so an invalid separator is still rejected before any data
        // can be written with it.
        ValidateSeparator(separator);
    }

    private static void ValidateSeparator(string separator)
    {
        ArgumentNullException.ThrowIfNull(separator);

        if (separator.Length == 0)
        {
            throw new ArgumentException("The separator must not be empty.", nameof(separator));
        }

        if (!ContainsCharacterOutsideEscapedOutput(separator))
        {
            throw new ArgumentException($"The separator \"{separator}\" consists only of characters that can appear in escaped element data, so an " +
                                        "element could not be distinguished from a delimiter on read. Uri.EscapeDataString emits the unreserved " +
                                        "characters A-Z, a-z, 0-9, '-', '.', '_' and '~' literally, and '%' as the escape introducer. The separator " +
                                        "must contain at least one character outside that set — the default \",\" does.",
                                        nameof(separator));
        }
    }

    /// <summary>
    ///     Determines whether the separator contains a character that can never occur in escaped
    ///     element data, which is what makes it safe as a delimiter.
    /// </summary>
    /// <remarks>
    ///     <see cref="Uri.EscapeDataString(string)" /> output is drawn from exactly two sources: the
    ///     RFC 3986 unreserved characters, emitted literally, and percent-triplets, which introduce
    ///     <c>%</c> and hexadecimal digits. A separator built only from those characters can appear
    ///     inside an escaped element — <c>%</c> occurs in every escaped character, and <c>%2C</c>
    ///     is exactly how a comma is escaped — so splitting on it would tear elements apart. Testing
    ///     whether escaping merely <i>changes</i> the separator is not sufficient: <c>%2C</c> escapes
    ///     to <c>%252C</c> yet still collides with the escaped form of <c>,</c>.
    /// </remarks>
    /// <param name="separator">The separator to test.</param>
    /// <returns>
    ///     <see langword="true" /> if at least one character of <paramref name="separator" /> cannot
    ///     appear in escaped element data; otherwise <see langword="false" />.
    /// </returns>
    private static bool ContainsCharacterOutsideEscapedOutput(string separator) => separator.Any(character => !IsEscapedOutputCharacter(character));

    private static bool IsEscapedOutputCharacter(char character) =>
        char.IsAsciiLetterOrDigit(character) || character == '-' || character == '.' || character == '_' || character == '~' || character == '%';

    private static string? Serialize(ICollection<TValue>? values, string separator) =>
        values is null
            ? null
            : string.Join(separator,
                          values.Select(v => v is not null
                                            ? Uri.EscapeDataString(Convert.ToString(v, CultureInfo.InvariantCulture)!)
                                            : string.Empty));

    private static List<TValue>? Deserialize(string? value, string separator)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Length == 0)
        {
            return [];
        }

        // Empty entries must be preserved — an empty segment is how a null element is encoded — so
        // this relies on Split's default StringSplitOptions.None. Passing it explicitly would be
        // redundant (S3254); removing entries here would silently drop nulls.
        return
        [
            .. value.Split(separator)
                    .Select(v => v.Length == 0
                                ? default!
                                : (TValue)Convert.ChangeType(Uri.UnescapeDataString(v), typeof(TValue), CultureInfo.InvariantCulture)),
        ];
    }
}
