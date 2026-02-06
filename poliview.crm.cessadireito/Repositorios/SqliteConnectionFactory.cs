using Microsoft.Data.Sqlite;

namespace poliview.crm.cessadireito.Repositorios
{
    public class SqliteConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());
        }

        public SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
