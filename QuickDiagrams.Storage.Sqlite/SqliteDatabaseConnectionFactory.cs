using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Common;

namespace QuickDiagrams.Storage.Sqlite
{
    public class SqliteDatabaseConnectionFactory
        : IDatabaseConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteDatabaseConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }

        public DbConnection Create()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}