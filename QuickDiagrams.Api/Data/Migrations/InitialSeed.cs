using Dapper;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Api.Data.Migrations
{
    public class InitialSeed
        : IDataMigration
    {
        public int Version => 0;

        public async Task RunAsync(DbConnection connection, CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                @"CREATE TABLE IF NOT EXISTS [ApplicationUser]
                    (
                        [Id] INTEGER NOT NULL PRIMARY KEY,
                        [UserName] TEXT NOT NULL,
                        [NormalizedUserName] TEXT NOT NULL,
                        [Email] TEXT NULL,
                        [NormalizedEmail] TEXT NULL,
                        [EmailConfirmed] INTEGER NOT NULL,
                        [PasswordHash] TEXT NULL,
                        [PhoneNumber] TEXT NULL,
                        [PhoneNumberConfirmed] INTEGER NOT NULL,
                        [TwoFactorEnabled] INTEGER NOT NULL
                    )"
            );
        }
    }
}