using FirebirdSql.Data.FirebirdClient;

namespace poliview.crm.cessadireito.Repositorios
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
            var connection = new FbConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
