using FirebirdSql.Data.FirebirdClient;

namespace Poliview.crm.services
{
    public class FirebirdConnectionService
    {
        private readonly string _connectionString;

        public FirebirdConnectionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public FbConnection CreateConnection()
        {
            var connection = new FbConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
