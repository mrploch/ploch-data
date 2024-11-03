using System.Data;

namespace Ploch.Data.Utilities;

/// <summary>
///     Provides extension methods for the <see cref="System.Data.DataColumn" /> class.
/// </summary>
public static class DataColumnExtensions
{
    /// <summary>
    ///     Copies the properties from the source <see cref="DataColumn" /> to the target <see cref="DataColumn" />.
    /// </summary>
    /// <param name="sourceColumn">The <see cref="DataColumn" /> from which to copy properties.</param>
    /// <param name="targetColumn">The <see cref="DataColumn" /> to which properties will be copied.</param>
    public static void CopyProperties(this DataColumn sourceColumn, DataColumn targetColumn)
    {
        targetColumn.AllowDBNull = sourceColumn.AllowDBNull;
        targetColumn.AutoIncrement = sourceColumn.AutoIncrement;
        targetColumn.Caption = sourceColumn.Caption;
        targetColumn.AutoIncrementSeed = sourceColumn.AutoIncrementSeed;
        targetColumn.AutoIncrementStep = sourceColumn.AutoIncrementStep;
    }
}