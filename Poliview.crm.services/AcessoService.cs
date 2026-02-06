using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public class AcessoService : IAcessoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public AcessoService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            var connectionString = configuration["conexao"];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("conexao", "Connection string 'conexao' não encontrada na configuração");
            _connectionString = connectionString;
        }

        public Acesso Listar(string chaveacesso)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select * from OPE_CONFIG where chaveacesso=@chaveacesso";
            Console.WriteLine(query);
            return connection.QueryFirst<Acesso>(query, new { chaveacesso });
        }
    }
}
