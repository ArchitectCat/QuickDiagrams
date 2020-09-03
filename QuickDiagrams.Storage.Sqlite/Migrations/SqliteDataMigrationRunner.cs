using Dapper;
using QuickDiagrams.Storage.Migrations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickDiagrams.Storage.Sqlite.Migrations
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

            using (var connection = _connectionFactory.Create())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(cancellationToken);

                var allMigrations = migrations.Select(x => new DataMigrationInfo
                {
                    Version = x.Version
                }).ToList();

                await connection.ExecuteAsync(
                    @"CREATE TABLE IF NOT EXISTS [Migrations]
                        (
                            [Version] INTEGER NOT NULL
                        )"
                );

                var pastMigrations = await connection.QueryAsync<DataMigrationInfo>(
                    @"SELECT [Version] FROM [Migrations]"
                );

                var futureMigrations = allMigrations.Except(pastMigrations);
                if (!futureMigrations.Any())
                    return;

                foreach (var m in futureMigrations.OrderBy(x => x.Version))
                {
                    using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                    {
                        try
                        {
                            var migration = migrations.Single(x => x.Version == m.Version);

                            await migration.RunAsync(connection, cancellationToken);

                            await connection.ExecuteAsync(
                                @"INSERT INTO [Migrations]([Version]) VALUES(@Version)",
                                new { Version = m.Version }
                            );

                            await transaction.CommitAsync(cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            throw ex;
                        }
                    }
                }
            }
        }
    }
}