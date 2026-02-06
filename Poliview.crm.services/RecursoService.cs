using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.Data.SqlClient;
using Poliview.crm.models;
using Poliview.crm.domain;

namespace Poliview.crm.services
{
    public interface IRecursoService
    {
        public ListarRecursosResposta ListAll();
        public ListarRecursosResposta GetByIdGrupo(int idgrupo);
        public List<Recurso> ListaRecursosPorGrupo(int idgrupo);
    }

    public class RecursoService : IRecursoService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public RecursoService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarRecursosResposta ListAll()
        {
            var retorno = new ListarRecursosResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "SELECT * FROM dbo.vUsuariosGrupo ";
                var result = connection.Query<Recurso>(query);
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

        public ListarRecursosResposta GetByIdGrupo(int idgrupo)
        {
            var retorno = new ListarRecursosResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"SELECT * FROM dbo.vUsuariosGrupo where CD_grupo ={idgrupo} ";
                var result = connection.Query<Recurso>(query);
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

        public List<Recurso> ListaRecursosPorGrupo(int idgrupo)
        {
            var ret = new List<Recurso>();
            using var connection = new SqlConnection(_connectionString);
            var query = $"SELECT * FROM dbo.vUsuariosGrupo where CD_grupo ={idgrupo} ";
            var result = connection.Query<Recurso>(query);
            return result.ToList();
        }
    }
}
