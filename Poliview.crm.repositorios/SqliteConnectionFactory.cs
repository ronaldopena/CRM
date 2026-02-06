using Microsoft.Data.Sqlite;

namespace Poliview.crm.repositorios
{
    public class SqliteConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string connectionString)
        {
            // SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
            _connectionString = connectionString;
        }

        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
