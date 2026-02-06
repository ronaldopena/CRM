using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public class SqlServerConnectionService
    {
        private readonly string _connectionString;

        public SqlServerConnectionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
