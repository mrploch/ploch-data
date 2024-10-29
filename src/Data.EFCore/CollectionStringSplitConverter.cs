using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Ploch.Data.EFCore;

/// <summary>
///     Converts a collection of values to a delimited string and vice versa.
/// </summary>
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
    /// <typeparam name="TValue">The type of the elements in the collection.</typeparam>
    /// <param name="separator">Separator to be used when converting the collection to string.</param>
    /// <param name="convertNulls">Include null values in the conversion.</param>
    /// <param name="mappingHints">Optional mapping hints to pass to the baase converter.</param>
    public CollectionStringSplitConverter(string separator = ",", bool convertNulls = true, ConverterMappingHints? mappingHints = null) :
        base(values => string.Join(separator, values.Select(v => v != null ? Uri.EscapeDataString(v.ToString()!) : string.Empty)),
             s => (ICollection<TValue>)Uri.UnescapeDataString(s)
                                          .Split(separator, StringSplitOptions.None)
                                          .Select(v => Convert.ChangeType(v, typeof(TValue))),
             convertNulls, mappingHints)
#pragma warning restore EF1001
    { }
}