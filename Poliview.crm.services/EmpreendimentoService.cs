using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IEmpreendimentoService
    {
        public ListarEmpreendimentosResposta Listar();
        public ListarEmpreendimentosResposta ListarParaRelatorios();
        public ListarEmpreendimentoPorIdResposta ListaPorId(int id);
        public ListarEmpreendimentoPorIdResposta ListaPorIdSp7(int idemprdsp7);
        public EmpreendimentoResposta Update(Empreendimento empreendimento);
    }

    public class EmpreendimentoService : IEmpreendimentoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public EmpreendimentoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarEmpreendimentoPorIdResposta ListaPorId(int id)
        {
            var retorno = new ListarEmpreendimentoPorIdResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select cd_empreendimento as id, cd_empreeSP7 as idemprdsp7, NM_empree as nome, mostraRelatorioEspacoCliente from cad_empreendimento where id={id}";

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.QueryFirst<Empreendimento>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }

        public ListarEmpreendimentoPorIdResposta ListaPorIdSp7(int idemprdsp7)
        {
            var retorno = new ListarEmpreendimentoPorIdResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select cd_empreendimento as id, cd_empreeSP7 as idemprdsp7, NM_empree as nome, mostraRelatorioEspacoCliente from cad_empreendimento where cd_empreeSP7={idemprdsp7}";

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.QueryFirst<Empreendimento>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;
        }

        public ListarEmpreendimentosResposta Listar()
        {
            var retorno = new ListarEmpreendimentosResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select cd_empreendimento as id, cd_empreeSP7 as idemprdsp7, NM_empree as nome, mostraRelatorioEspacoCliente from cad_empreendimento order by nome";
                
                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<Empreendimento>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }

        public ListarEmpreendimentosResposta ListarParaRelatorios()
        {
            var retorno = new ListarEmpreendimentosResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select cd_empreendimento as id, cd_empreeSP7 as idemprdsp7, NM_empree as nome, mostraRelatorioEspacoCliente from cad_empreendimento where mostraRelatorioEspacoCliente=1 order by nome";

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<Empreendimento>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }


        public EmpreendimentoResposta Update(Empreendimento empreendimento)
        {
            var retorno = new EmpreendimentoResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "UPDATE CAD_EMPREENDIMENTO SET " +
                            $"cd_empreeSP7={empreendimento.idemprdsp7}, " +
                            $"NM_empree='{empreendimento.nome}', " +
                            $"mostraRelatorioEspacoCliente={empreendimento.mostraRelatorioEspacoCliente} " +
                            $"where CD_Empreendimento={empreendimento.id}";
                connection.Execute(query);
                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = empreendimento;
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
