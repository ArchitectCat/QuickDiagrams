using System;

namespace QuickDiagrams.Storage.Sqlite.TypeHandlers
{
    public class DateTimeOffsetTypeHandler
        : SqliteTypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
            => DateTimeOffset.Parse((string)value);
    }
}