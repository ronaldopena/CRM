using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IUnidadeService
    {
        public ListarUnidadesResposta Listar(int idempreendimento, int idbloco);
        public ListarUnidadesResposta ListarParaRelatorios(int idempreendimento, int idbloco);
    }

    public class UnidadeService : IUnidadeService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public UnidadeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarUnidadesResposta Listar(int idempreendimento, int idbloco)
        {
            var retorno = new ListarUnidadesResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                // $"select cd_unidade as id, CD_EmpreeSP7 as idemprdsp7, CD_BlocoSP7, nr_unidadesp7 as nome from CAD_UNIDADE where CD_Empreendimento={idempreendimento} and cd_Bloco={idbloco}";

                var query = $"select uni.cd_unidade as id, emp.CD_EmpreeSP7 as idemprdsp7, emp.CD_Empreendimento as idempreendimento, blo.CD_Bloco as idbloco, blo.CD_BlocoSP7 as idblocosp7, nr_unidadesp7 as nome  from CAD_UNIDADE uni " +
                $"left join CAD_EMPREENDIMENTO emp on emp.CD_EmpreeSP7 = uni.CD_EmpreeSP7 " +
                $"left join CAD_BLOCO blo on blo.CD_EmpreeSP7 = emp.CD_EmpreeSP7 and blo.CD_BlocoSP7 = uni.CD_BlocoSP7 " +
                $"where emp.CD_Empreendimento = {idempreendimento} and blo.CD_Bloco = {idbloco} ";

                Console.WriteLine(query);

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<Unidade>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }

        public ListarUnidadesResposta ListarParaRelatorios(int idempreendimento, int idbloco)
        {
            var retorno = new ListarUnidadesResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                // $"select cd_unidade as id, CD_EmpreeSP7 as idemprdsp7, CD_BlocoSP7, nr_unidadesp7 as nome from CAD_UNIDADE where CD_Empreendimento={idempreendimento} and cd_Bloco={idbloco}";

                var query = $"select uni.cd_unidade as id, emp.CD_EmpreeSP7 as idemprdsp7, emp.CD_Empreendimento as idempreendimento, blo.CD_Bloco as idbloco, blo.CD_BlocoSP7 as idblocosp7, nr_unidadesp7 as nome  from CAD_UNIDADE uni " +
                $"left join CAD_TIPO_UNIDADE ctu on ctu.id = uni.tipo " +
                $"left join CAD_EMPREENDIMENTO emp on emp.CD_EmpreeSP7 = uni.CD_EmpreeSP7 " +
                $"left join CAD_BLOCO blo on blo.CD_EmpreeSP7 = emp.CD_EmpreeSP7 and blo.CD_BlocoSP7 = uni.CD_BlocoSP7 " +
                $"where emp.CD_Empreendimento = {idempreendimento} and blo.CD_Bloco = {idbloco} and ctu.espacocliente = 1";

                Console.WriteLine(query);

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<Unidade>(query);
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
