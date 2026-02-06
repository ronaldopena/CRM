using System.Data.SqlClient;
using Dapper;
using apimobuss.Entidades;

namespace apimobuss.Repositorio
{
    public class TesteRepositorio
    {
        private readonly string _connectionString;

        public TesteRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public object testar()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = string.Format("EXEC CRM_Config");
            var result = connection.Query(query);
            return result;
        }
    }
}
