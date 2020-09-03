using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Api.Data
{
    public interface IDataMigration
    {
        int Version { get; }

        Task RunAsync(DbConnection connection, CancellationToken cancellationToken);
    }
}