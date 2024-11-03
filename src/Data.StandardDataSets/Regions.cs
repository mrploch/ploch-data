using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ploch.Data.StandardDataSets;

/// <summary>
///     The Regions class provides methods to retrieve information about
///     various regions and their associated cultures.
/// </summary>
public static class Regions
{
    /// <summary>
    ///     Retrieves a collection of tuples, each containing a <see cref="CultureInfo" />
    ///     object and its corresponding <see cref="RegionInfo" /> object.
    /// </summary>
    /// <returns>
    ///     An IEnumerable of tuples where each tuple consists of:
    ///     - <see cref="CultureInfo" />: representing the specific culture.
    ///     - <see cref="RegionInfo" />: representing the associated region.
    /// </returns>
    public static IEnumerable<Tuple<CultureInfo, RegionInfo>> GetRegions()
    {
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(ci => Tuple.Create(ci, new RegionInfo(ci.Name)));
    }

    /// <summary>
    ///     Retrieves a distinct and ordered collection of English country names
    ///     derived from the available <see cref="RegionInfo" /> objects.
    /// </summary>
    /// <returns>
    ///     An IEnumerable of strings where each string represents the English
    ///     name of a country, ordered alphabetically.
    /// </returns>
    public static IEnumerable<string> EnglishCountryNames()
    {
        return GetRegions().Select(region => region.Item2.EnglishName).Distinct().OrderBy(name => name);
    }
}