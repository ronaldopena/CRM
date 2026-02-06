
using Microsoft.Data.SqlClient;

namespace Poliview.crm.repositorios
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
            try
            {
                // Console.WriteLine("SQL Connection = " + _connectionString);
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
