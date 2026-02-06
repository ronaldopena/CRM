using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface IBlocoService
    {
        public ListarBlocosResposta Listar(int idempreendimento);        
    }

    public class BlocoService : IBlocoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;
        
        public BlocoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarBlocosResposta Listar(int idempreendimento)
        {
            var retorno = new ListarBlocosResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select cd_bloco as id, cd_empreeSp7 as idemprdsp7, cd_blocoSp7 as idblocosp7, nm_bloco as nome from cad_bloco where CD_EmpreeSp7=(select CD_EmpreeSP7 from CAD_EMPREENDIMENTO where CD_Empreendimento={idempreendimento})";

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<Bloco>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }
    }
}
