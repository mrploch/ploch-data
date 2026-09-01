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
    ///     The types <see cref="Convert.ChangeType(object, Type, IFormatProvider)" /> can actually
    ///     produce <i>from a <see cref="string" /></i>.
    /// </summary>
    /// <remarks>
    ///     Implementing <see cref="IConvertible" /> is not sufficient to be decodable. Conversion runs
    ///     against the <i>source</i> string, so <see cref="Convert.ChangeType(object, Type, IFormatProvider)" />
    ///     dispatches to <see cref="string" />'s own implementation, which recognises only this fixed
    ///     set and throws <see cref="InvalidCastException" /> for any other target — including a
    ///     user-defined type that implements <see cref="IConvertible" />. Testing assignability to
    ///     <see cref="IConvertible" /> would therefore accept types that can be written but never read.
    /// </remarks>
    private static readonly HashSet<Type> ConvertibleTargets =
        [
            typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
            typeof(ulong), typeof(float), typeof(double), typeof(decimal),
        ];

    /// <summary>
    ///     Determines whether an element of the supplied type can be both written and read back
    ///     faithfully.
    /// </summary>
    /// <remarks>
    ///     This is keyed on the <i>declared</i> element type, which is what the read path has to work
    ///     from. <see cref="Encode" /> dispatches on the runtime value, so without this check a
    ///     converter declared over an unsupported type — <see cref="object" />, an interface, or a
    ///     custom <see cref="IConvertible" /> — could write a payload it is then unable to read.
    /// </remarks>
    /// <param name="elementType">
    ///     The element type, already unwrapped from <see cref="Nullable{T}" /> by the caller.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the type round-trips; otherwise <see langword="false" />.
    /// </returns>
    internal static bool IsSupported(Type elementType) =>
        Decoders.ContainsKey(elementType) || elementType.IsEnum || ConvertibleTargets.Contains(elementType);

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

        if (ConvertibleTargets.Contains(elementType))
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
        $"Elements of type {elementType} are not supported by CollectionStringSplitConverter. Supported element types are string, the built-in " +
        "primitives (bool, char, the integral and floating-point types, decimal), Guid, TimeSpan, DateTime, DateTimeOffset, DateOnly, TimeOnly, any " +
        "enum, and Nullable of any of those. Implementing IConvertible is not sufficient: a value is decoded by converting the stored string, which " +
        "cannot produce a user-defined target type.";
}
