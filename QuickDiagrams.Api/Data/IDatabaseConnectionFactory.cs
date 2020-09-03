using System.Data.Common;

namespace QuickDiagrams.Api.Data
{
    public interface IDatabaseConnectionFactory
    {
        DbConnection Create();
    }
}