using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Storage.Migrations
{
    public interface IDataMigrationRunner
    {
        Task RunMigrationsAsync(IEnumerable<IDataMigration> migrations, CancellationToken cancellationToken);
    }
}