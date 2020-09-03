using System;

namespace QuickDiagrams.Api.Data.TypeHandlers
{
    public class DateTimeOffsetTypeHandler
        : SqliteTypeHandler<DateTimeOffset>
    {
        public override DateTimeOffset Parse(object value)
            => DateTimeOffset.Parse((string)value);
    }
}