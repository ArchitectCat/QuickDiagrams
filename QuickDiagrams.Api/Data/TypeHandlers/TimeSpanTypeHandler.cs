using System;

namespace QuickDiagrams.Api.Data.TypeHandlers
{
    public class TimeSpanTypeHandler
        : SqliteTypeHandler<TimeSpan>
    {
        public override TimeSpan Parse(object value)
            => TimeSpan.Parse((string)value);
    }
}