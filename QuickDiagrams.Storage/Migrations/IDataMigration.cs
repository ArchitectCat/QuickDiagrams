using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Storage.Migrations
{
    public interface IDataMigration
    {
        int Version { get; }

        Task RunAsync(DbConnection connection, CancellationToken cancellationToken);
    }
}