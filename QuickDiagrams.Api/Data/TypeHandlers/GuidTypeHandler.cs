using System;

namespace QuickDiagrams.Api.Data.TypeHandlers
{
    public class GuidTypeHandler
        : SqliteTypeHandler<Guid>
    {
        public override Guid Parse(object value)
            => Guid.Parse((string)value);
    }
}