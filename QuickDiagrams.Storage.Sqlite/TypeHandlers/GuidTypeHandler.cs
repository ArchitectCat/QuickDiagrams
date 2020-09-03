using System;

namespace QuickDiagrams.Storage.Sqlite.TypeHandlers
{
    public class GuidTypeHandler
        : SqliteTypeHandler<Guid>
    {
        public override Guid Parse(object value)
            => Guid.Parse((string)value);
    }
}