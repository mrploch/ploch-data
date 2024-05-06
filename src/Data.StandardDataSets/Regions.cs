using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ploch.Common.Data.StandardDataSets
{
    public static class Regions
    {
        public static IEnumerable<Tuple<CultureInfo, RegionInfo>> GetRegions()
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(ci => Tuple.Create(ci, new RegionInfo(ci.Name)));
        }

        public static IEnumerable<string> EnglishCountryNames()
        {
            return GetRegions().Select(region => region.Item2.EnglishName).Distinct().OrderBy(name => name);
        }
    }
}