using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ploch.Data.EFCore;

/// <summary>
///     Converts a collection of values to a delimited string and vice versa.
/// </summary>
/// <remarks>
///     <para>
///         <b>Wire format (version 1).</b> A non-<see langword="null" /> collection is written as the
///         two-character header <c>!1</c> followed by one <i>separator-introduced</i> segment per
///         element. Every element — including the first — is preceded by the separator, so the header
///         alone (<c>!1</c>) means an empty collection and no element is ever encoded as an empty
///         segment. Each segment carries a mandatory one-character tag: <c>n</c> for a
///         <see langword="null" /> element, or <c>v</c> followed by the element's round-trip
///         representation escaped with <see cref="Uri.EscapeDataString(string)" />.
///     </para>
///     <para>
///         The tag makes <see langword="null" /> and <see cref="string.Empty" /> distinguishable, which
///         the previous format could not do: <c>["a", ""]</c> is written as <c>!1,va,v</c> and
///         <c>["a", null]</c> as <c>!1,va,n</c>, while <c>[""]</c> (<c>!1,v</c>), <c>[null]</c>
///         (<c>!1,n</c>) and <c>[]</c> (<c>!1</c>) are three distinct payloads.
///     </para>
///     <para>
///         The <c>!</c> sentinel is safe because <see cref="Uri.EscapeDataString(string)" /> output is
///         drawn only from the RFC 3986 unreserved characters — <c>A-Z</c>, <c>a-z</c>, <c>0-9</c>,
///         <c>-</c>, <c>.</c>, <c>_</c>, <c>~</c> — and percent-triplets, which introduce <c>%</c>.
///         <c>!</c> lies outside that alphabet (it escapes to <c>%21</c>), so escaped element data can
///         never begin with the header. The <c>v</c> and <c>n</c> tags are inside the alphabet but are
///         read positionally, as the first character of a segment whose boundaries the separator has
///         already fixed, so they cannot collide with element data either.
///     </para>
///     <para>
///         Values are serialised and deserialised using <see cref="CultureInfo.InvariantCulture" />,
///         so round-trips are stable regardless of the current thread culture. Element encodings are
///         chosen to be round-trip faithful: <see cref="DateTime" /> and
///         <see cref="DateTimeOffset" /> use the <c>"O"</c> round-trip format, which preserves both
///         sub-second precision and <see cref="DateTimeKind" />; <see cref="TimeSpan" /> uses
///         <c>"c"</c>; <see cref="Guid" /> uses <c>"D"</c>; <see cref="DateOnly" /> and
///         <see cref="TimeOnly" /> use <c>"O"</c>; enums are written by name; and every remaining
///         <see cref="IConvertible" /> type uses its invariant string form.
///     </para>
///     <para>
///         A <see langword="null" /> <i>collection</i> is distinct from an empty one: it is written
///         as <see langword="null" /> (a <c>NULL</c> column) and read back as <see langword="null" />,
///         whereas an empty collection is written as <c>!1</c> and read back as an empty collection.
///     </para>
///     <para>
///         <b>Legacy payloads are rejected, not guessed at.</b> A non-<see langword="null" /> payload
///         that does not begin with the <c>!1</c> header throws <see cref="FormatException" />. Reading
///         such a payload under the old rules would reintroduce exactly the ambiguity this format
///         removes — under those rules an empty segment meant <see langword="null" /> <i>and</i>
///         <see cref="string.Empty" /> — so a best-effort read would quietly hand back wrong data.
///         Failing loudly is safe here because the previous format never reached a released version,
///         and the format before that could not be read back at all: its read path threw
///         <see cref="InvalidCastException" /> for every payload and every
///         <typeparamref name="TValue" />, so no data this converter wrote was ever readable.
///     </para>
///     <para>
///         <b>The separator must contain at least one character that cannot occur in escaped
///         element data</b>, and this is enforced by the constructor. A separator built only from the
///         unreserved characters and <c>%</c> can appear inside an element and would tear it apart on
///         read: with <c>separator: "-"</c> the element <c>"a-b"</c> would be written as <c>a-b</c>,
///         and with <c>separator: "%2C"</c> an element containing a comma would be written as
///         <c>a%2Cb</c> — the separator's own spelling. The default <c>","</c> is safe, as is any
///         separator containing a reserved character such as <c>;</c> or <c>|</c>.
///     </para>
///     <para>
///         <typeparamref name="TValue" /> may be <see cref="string" />, any <see cref="IConvertible" />
///         type (the numeric primitives, <see cref="bool" />, <see cref="char" />,
///         <see cref="decimal" /> and <see cref="DateTime" />), <see cref="Guid" />,
///         <see cref="TimeSpan" />, <see cref="DateTimeOffset" />, <see cref="DateOnly" />,
///         <see cref="TimeOnly" />, any enum, or a <see cref="Nullable{T}" /> of any of those. A type
///         outside that set throws <see cref="NotSupportedException" /> on both write and read rather
///         than serialising into something that cannot be read back.
///     </para>
/// </remarks>
/// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
public class CollectionStringSplitConverter<TValue> : ValueConverter<ICollection<TValue>, string>
{
    /// <summary>
    ///     Marks the payload as version 1 of the tagged format. <c>!</c> cannot appear in escaped
    ///     element data, so the header can never be confused with a value.
    /// </summary>
    private const string FormatHeader = "!1";

    /// <summary>Tag introducing a segment that carries an element value.</summary>
    private const char ValueTag = 'v';

    /// <summary>Tag for a segment representing a <see langword="null" /> element.</summary>
    private const char NullTag = 'n';

    /// <summary>
    ///     The element type the codec works with: <typeparamref name="TValue" /> itself, or its
    ///     underlying type when <typeparamref name="TValue" /> is a <see cref="Nullable{T}" />. A
    ///     <see langword="null" /> element is carried by the <c>n</c> tag, so only the underlying
    ///     type is ever encoded or decoded.
    /// </summary>
    private static readonly Type ElementType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

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

    private static string? Serialize(ICollection<TValue>? values, string separator)
    {
        if (values is null)
        {
            return null;
        }

        var builder = new StringBuilder(FormatHeader);

        foreach (var value in values)
        {
            builder.Append(separator);

            if (value is null)
            {
                builder.Append(NullTag);
            }
            else
            {
                builder.Append(ValueTag).Append(Uri.EscapeDataString(CollectionElementCodec.Encode(value)));
            }
        }

        return builder.ToString();
    }

    private static List<TValue>? Deserialize(string? value, string separator)
    {
        if (value is null)
        {
            return null;
        }

        if (!value.StartsWith(FormatHeader, StringComparison.Ordinal))
        {
            throw new FormatException($"The stored value does not begin with the \"{FormatHeader}\" format header, so it was not written by this " +
                                      "converter's current format. Payloads in the earlier untagged format cannot be decoded unambiguously — an empty " +
                                      "segment meant both a null element and an empty string — so they are rejected rather than silently misread.");
        }

        var remainder = value[FormatHeader.Length..];

        if (remainder.Length == 0)
        {
            return [];
        }

        if (!remainder.StartsWith(separator, StringComparison.Ordinal))
        {
            throw new FormatException($"The stored value is malformed: every element is introduced by the separator \"{separator}\", so anything " +
                                      $"following the \"{FormatHeader}\" header must start with it.");
        }

        // Split's default StringSplitOptions.None is required: the payload always starts with a
        // separator, so the first entry is an empty string that is skipped, and every remaining
        // entry is a tagged segment, which is never empty in a well-formed payload.
        var segments = remainder.Split(separator);
        var values = new List<TValue>(segments.Length - 1);

        for (var index = 1; index < segments.Length; index++)
        {
            values.Add(DecodeSegment(segments[index]));
        }

        return values;
    }

    private static TValue DecodeSegment(string segment)
    {
        if (segment.Length > 0)
        {
            if (segment[0] == NullTag && segment.Length == 1)
            {
                return default!;
            }

            if (segment[0] == ValueTag)
            {
                return (TValue)CollectionElementCodec.Decode(Uri.UnescapeDataString(segment[1..]), ElementType);
            }
        }

        throw new FormatException($"The segment \"{segment}\" is malformed: every element segment is either \"{NullTag}\" for a null element or " +
                                  $"\"{ValueTag}\" followed by the escaped element value.");
    }
}
