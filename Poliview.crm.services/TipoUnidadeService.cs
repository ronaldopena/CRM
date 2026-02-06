using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface ITipoUnidadeService
    {
        public ListarTipoUnidadeResposta Listar();
        public ListarTipoUnidadePorIdResposta ListaPorId(int id);
        public TipoUnidadeResposta Update(TipoUnidade obj);
    }

    public class TipoUnidadeService : ITipoUnidadeService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public TipoUnidadeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarTipoUnidadePorIdResposta ListaPorId(int id)
        {
            var retorno = new ListarTipoUnidadePorIdResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select id, descricao, espacocliente from CAD_TIPO_UNIDADE where id={id}";

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.QueryFirst<TipoUnidade>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }
        
        public ListarTipoUnidadeResposta Listar()
        {
            var retorno = new ListarTipoUnidadeResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"select id, descricao, espacocliente from CAD_TIPO_UNIDADE order by descricao";
                
                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<TipoUnidade>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }

        public TipoUnidadeResposta Update(TipoUnidade obj)
        {
            var retorno = new TipoUnidadeResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"UPDATE CAD_TIPO_UNIDADE SET descricao='{obj.descricao}', espacocliente={obj.espacocliente} where id={obj.id}";
                connection.Execute(query);
                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = obj;
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
