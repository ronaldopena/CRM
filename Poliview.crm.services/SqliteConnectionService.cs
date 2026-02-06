using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.services
{
    public class SqliteConnectionService
    {
        private readonly string _connectionString;

        public SqliteConnectionService(string connectionString)
        {
            _connectionString = connectionString;
            // SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
        }

        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
