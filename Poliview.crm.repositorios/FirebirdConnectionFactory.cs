using FirebirdSql.Data.FirebirdClient;

namespace Poliview.crm.repositorios
{
    public class FirebirdConnectionFactory
    {
        private readonly string _connectionString;

        public FirebirdConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public FbConnection CreateConnection()
        {
            // Console.WriteLine("FB Connection = " + _connectionString);
            var connection = new FbConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
