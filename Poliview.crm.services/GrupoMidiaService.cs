using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Poliview.crm.models;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public interface IGrupoMidiaService
    {
        public ListarGrupoMidiaResposta ListAll();        
        public GrupoMidiaResposta FindById(int idgrupomidia);
        public GrupoMidiaResposta Create(GrupoMidia grupomidia);
        public GrupoMidiaResposta Update(GrupoMidia grupomidia);
        public GrupoMidiaResposta Delete(int idgrupomidia);
    }

    public class GrupoMidiaService : IGrupoMidiaService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public GrupoMidiaService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public GrupoMidiaResposta Create(GrupoMidia grupomidia)
        {
            var retorno = new GrupoMidiaResposta();
            try
            {   
                using var connection = new SqlConnection(_connectionString);
                var query = $"insert into CAD_GRUPO_MIDIA ([descricao],[linkimagem]) ";
                query += " values ";
                query += $"('{grupomidia.descricao}','{grupomidia.linkimagem}'); SELECT SCOPE_IDENTITY(); ";

                Console.WriteLine(query);
                var result = connection.QueryFirstOrDefault<int>(query);

                if (result>0)
                {
                    grupomidia.id = result;
                    retorno.mensagem = "OK";
                    retorno.objeto = grupomidia;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
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

        public GrupoMidiaResposta Delete(int idgrupomidia)
        {
            var retorno = new GrupoMidiaResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"delete from CAD_GRUPO_MIDIA where id=${idgrupomidia} ";
                var result = connection.Execute(query);
                if (result > 0)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Grupo mídia não encontrado";
                    retorno.objeto = null;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
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

        public GrupoMidiaResposta FindById(int idgrupomidia)
        {
            var retorno = new GrupoMidiaResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var query = $"SELECT * FROM CAD_GRUPO_MIDIA WHERE id={idgrupomidia}";
                var result = connection.QueryFirstOrDefault<GrupoMidia>(query);
                if (result != null)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "Grupo Mídia não encontrado";
                    retorno.objeto = result;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
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

        public ListarGrupoMidiaResposta ListAll()
        {
            var retorno = new ListarGrupoMidiaResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = "SELECT * FROM CAD_GRUPO_MIDIA ";
                var result = connection.Query<GrupoMidia>(query);
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
        public GrupoMidiaResposta Update(GrupoMidia grupomidia)
        {
            var retorno = new GrupoMidiaResposta();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var query = $"update CAD_GRUPO_MIDIA set " +
                            $"[descricao]='{grupomidia.descricao}',"+
                            $"[linkimagem]='{grupomidia.linkimagem}' "+
                            $"where [id]={grupomidia.id}";

                Console.WriteLine(query);

                var result = connection.Execute(query);
                
                if (result > 0)
                {
                    retorno.mensagem = "OK";
                    retorno.objeto = grupomidia;
                    retorno.status = 200;
                    retorno.sucesso = true;
                }
                else
                {
                    retorno.mensagem = "grupo mídia não encontrada";
                    retorno.objeto = grupomidia;
                    retorno.status = 200;
                    retorno.sucesso = false;
                }
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
