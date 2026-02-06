using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public class OcorrenciasAberturaChamadoService : IOcorrenciasAberturaChamadoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public OcorrenciasAberturaChamadoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListaOcorrenciasAberturaChamadoResposta RetornaOcorrenciasAberturaChamado()
        {
            var retorno = new ListaOcorrenciasAberturaChamadoResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "EXEC CRM_Listar_Ocorrencias_Abertura_chamado ";
                var result = connection.Query<OcorrenciasAberturaChamado>(query);
                retorno.objeto = result;
                retorno.mensagem = "Ok";
                retorno.status = 200;                     
            }
            catch (Exception ex)
            {
                retorno.objeto = null;
                retorno.mensagem = ex.Message;
                retorno.status = 500;
            }


            return retorno;
        }
    }
}
