using Dapper;
using System.Data;

namespace QuickDiagrams.Storage.Sqlite.TypeHandlers
{
    public abstract class SqliteTypeHandler<T>
        : SqlMapper.TypeHandler<T>
    {
        public override void SetValue(IDbDataParameter parameter, T value)
            => parameter.Value = value;
    }
}