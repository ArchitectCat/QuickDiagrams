using System;

namespace QuickDiagrams.Storage.Sqlite.TypeHandlers
{
    public class TimeSpanTypeHandler
        : SqliteTypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value)
            => TimeSpan.Parse((string)value);
    }
}