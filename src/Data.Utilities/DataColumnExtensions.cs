using System.Data;

namespace Ploch.Common.Data.Utilities
{
    public static class DataColumnExtensions
    {
        public static void CopyProperties(this DataColumn sourceColumn, DataColumn targetColumn)
        {
            targetColumn.AllowDBNull = sourceColumn.AllowDBNull;
            targetColumn.AutoIncrement = sourceColumn.AutoIncrement;
            targetColumn.Caption = sourceColumn.Caption;
            targetColumn.AutoIncrementSeed = sourceColumn.AutoIncrementSeed;
            targetColumn.AutoIncrementStep = sourceColumn.AutoIncrementStep;
        }
    }
}