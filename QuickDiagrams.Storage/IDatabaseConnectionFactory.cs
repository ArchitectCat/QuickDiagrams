using System.Data.Common;

namespace QuickDiagrams.Storage
{
    public interface IDatabaseConnectionFactory
    {
        DbConnection Create();
    }
}