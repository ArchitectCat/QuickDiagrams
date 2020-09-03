using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Api.Data
{
    public class SqliteDataMigrationRunner
        : IDataMigrationRunner
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;

        public SqliteDataMigrationRunner(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task RunMigrationsAsync(IEnumerable<IDataMigration> migrations, CancellationToken cancellationToken)
        {
            if (!migrations.Any())
                return;

            using (var conn = _connectionFactory.Create())
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync(cancellationToken);

                var allMigrations = migrations.Select(x => new DataMigrationInfo
                {
                    Version = x.Version
                }).ToList();

                await conn.ExecuteAsync(
                    @"CREATE TABLE IF NOT EXISTS [Migrations]
                        (
                            [Version] INTEGER NOT NULL
                        )"
                );

                var pastMigrations = await conn.QueryAsync<DataMigrationInfo>(
                    @"SELECT [Version] FROM [Migrations]"
                );

                var futureMigrations = allMigrations.Except(pastMigrations);
                if (!futureMigrations.Any())
                    return;

                foreach (var m in futureMigrations.OrderBy(x => x.Version))
                {
                    using (var tran = await conn.BeginTransactionAsync(cancellationToken))
                    {
                        try
                        {
                            var migration = migrations.Single(x => x.Version == m.Version);

                            await migration.RunAsync(conn, cancellationToken);

                            await conn.ExecuteAsync(
                                @"INSERT INTO [Migrations]([Version]) VALUES(@Version)",
                                new { Version = m.Version }
                            );

                            await tran.CommitAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await tran.RollbackAsync(cancellationToken);
                            throw ex;
                        }
                    }
                }
            }
        }
    }
}