using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IGrupoService
    {
        public ListarGruposResposta Listar();
    }
    public class GrupoService : IGrupoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public GrupoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarGruposResposta Listar()
        {
            var retorno = new ListarGruposResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "SELECT * FROM OPE_GRUPO where in_status ='A' ";
                var result = connection.Query<Grupo>(query);
                retorno.mensagem = "OK";
                retorno.objeto = result;
                retorno.status = 200;
                retorno.sucesso = true;
                return retorno;
            }
            catch (Exception e)
            {
                retorno.mensagem = e.Message;
                retorno.objeto = null;
                retorno.status = 500;
                retorno.sucesso = false;
                return retorno;
            }
        }
    }
}
