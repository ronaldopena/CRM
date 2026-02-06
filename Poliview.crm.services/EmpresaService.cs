using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface IEmpresaService
    {
        public ListarEmpresasResposta Listar();
        public ListarEmpresaResposta ListarPorDominio(string dominio);
    }

    public class EmpresaService : IEmpresaService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;
        
        public EmpresaService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ListarEmpresasResposta Listar()
        {
            var retorno = new ListarEmpresasResposta();

            try
            {                
                using var connection = new SqlConnection(_connectionString);
                var query = $"select * from cad_empresa ";

                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.Query<Empresa>(query);
            }
            catch (Exception ex)
            {
                retorno.mensagem = ex.Message;
                retorno.sucesso = false;
                retorno.objeto = null;
            }

            return retorno;

        }

        public ListarEmpresaResposta ListarPorDominio(string dominio)
        {
            var retorno = new ListarEmpresaResposta();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var querycount = $"select count(*) as qtde from cad_empresa";
                var registros = connection.QueryFirst<int>(querycount);                
                Console.WriteLine($"registro de dominio = {registros.ToString()}");
                var query = $"select * from cad_empresa where dominioempresa=@dominio";

                if (registros==1) query = $"select top 1 * from cad_empresa";
                Console.WriteLine(query);
                
                retorno.mensagem = "ok";
                retorno.sucesso = true;
                retorno.objeto = connection.QueryFirstOrDefault<Empresa>(query, new { dominio=dominio });

                if (retorno.objeto == null)
                {
                    retorno.sucesso = false;
                    retorno.mensagem = $"Empresa não encontrada para o dominio {dominio}";
                }
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
