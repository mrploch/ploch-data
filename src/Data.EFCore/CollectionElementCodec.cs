using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ploch.Data.EFCore;

/// <summary>
///     Round-trip-faithful conversion between a single collection element and its textual form, used
///     by <see cref="CollectionStringSplitConverter{TValue}" />.
/// </summary>
/// <remarks>
///     <para>
///         Deliberately non-generic. The decoder table does not depend on the converter's type
///         parameter, so holding it in a generic type would allocate a separate copy for every closed
///         construction (Sonar S2743) without any benefit.
///     </para>
///     <para>
///         Every encoding here is chosen so that decoding restores the original value exactly.
///         <see cref="Convert.ToString(object, IFormatProvider)" /> is not sufficient on its own: for
///         <see cref="DateTime" /> it uses the general format, which has neither a fractional-seconds
///         field nor an offset, so both sub-second precision and <see cref="DateTimeKind" /> are lost
///         silently; and <see cref="Convert.ChangeType(object, Type, IFormatProvider)" /> cannot read
///         back <see cref="Guid" />, <see cref="TimeSpan" />, <see cref="DateTimeOffset" />,
///         <see cref="DateOnly" />, <see cref="TimeOnly" /> or enums at all.
///     </para>
/// </remarks>
internal static class CollectionElementCodec
{
    /// <summary>The .NET round-trip format specifier, used for the date and time types.</summary>
    private const string RoundTripFormat = "O";

    private static readonly Dictionary<Type, Func<string, object>> Decoders =
        new()
        {
            [typeof(string)] = text => text,
            [typeof(DateTime)] = text => DateTime.ParseExact(text, RoundTripFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            [typeof(DateTimeOffset)] = text => DateTimeOffset.ParseExact(text, RoundTripFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            [typeof(TimeSpan)] = text => TimeSpan.ParseExact(text, "c", CultureInfo.InvariantCulture),
            [typeof(Guid)] = text => Guid.ParseExact(text, "D"),
            [typeof(DateOnly)] = text => DateOnly.ParseExact(text, RoundTripFormat, CultureInfo.InvariantCulture),
            [typeof(TimeOnly)] = text => TimeOnly.ParseExact(text, RoundTripFormat, CultureInfo.InvariantCulture),
        };

    /// <summary>
    ///     Encodes a non-<see langword="null" /> element as the text stored in a value segment.
    /// </summary>
    /// <param name="value">The element to encode.</param>
    /// <returns>The element's round-trip textual representation, before escaping.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when the element's type has no round-trip encoding.
    /// </exception>
    internal static string Encode(object value) =>
        value switch
        {
            string text => text,
            DateTime dateTime => dateTime.ToString(RoundTripFormat, CultureInfo.InvariantCulture),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString(RoundTripFormat, CultureInfo.InvariantCulture),
            TimeSpan timeSpan => timeSpan.ToString("c", CultureInfo.InvariantCulture),
            Guid guid => guid.ToString("D", CultureInfo.InvariantCulture),
            DateOnly dateOnly => dateOnly.ToString(RoundTripFormat, CultureInfo.InvariantCulture),
            TimeOnly timeOnly => timeOnly.ToString(RoundTripFormat, CultureInfo.InvariantCulture),
            Enum enumeration => enumeration.ToString(),
            IConvertible convertible => convertible.ToString(CultureInfo.InvariantCulture),
            _ => throw new NotSupportedException(UnsupportedMessage(value.GetType())),
        };

    /// <summary>
    ///     Decodes the text of a value segment back into an element of the requested type.
    /// </summary>
    /// <param name="text">The unescaped segment text.</param>
    /// <param name="elementType">
    ///     The element type. For a <see cref="Nullable{T}" /> element this is the underlying type,
    ///     because a <see langword="null" /> element is carried by the segment tag instead.
    /// </param>
    /// <returns>The decoded element, boxed.</returns>
    /// <exception cref="NotSupportedException">
    ///     Thrown when <paramref name="elementType" /> has no round-trip decoding.
    /// </exception>
    internal static object Decode(string text, Type elementType)
    {
        if (Decoders.TryGetValue(elementType, out var decoder))
        {
            return decoder(text);
        }

        if (elementType.IsEnum)
        {
            return Enum.Parse(elementType, text);
        }

        if (typeof(IConvertible).IsAssignableFrom(elementType))
        {
            return Convert.ChangeType(text, elementType, CultureInfo.InvariantCulture);
        }

        throw new NotSupportedException(UnsupportedMessage(elementType));
    }

    /// <summary>
    ///     Builds the message shared by the encode and decode failure paths, so a type that cannot be
    ///     written is reported the same way as one that cannot be read.
    /// </summary>
    /// <param name="elementType">The unsupported element type.</param>
    /// <returns>The exception message.</returns>
    internal static string UnsupportedMessage(Type elementType) =>
        $"Elements of type {elementType} are not supported by CollectionStringSplitConverter. Supported element types are string, the IConvertible " +
        "primitives, Guid, TimeSpan, DateTime, DateTimeOffset, DateOnly, TimeOnly, any enum, and Nullable of any of those.";
}
