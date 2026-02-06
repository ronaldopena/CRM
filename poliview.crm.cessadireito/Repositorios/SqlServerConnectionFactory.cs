using System.Data.SqlClient;

namespace poliview.crm.cessadireito.Repositorios
{
    public class SqlServerConnectionFactory
    {
        private readonly string _connectionString;

        public SqlServerConnectionFactory(string connectionString)
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
