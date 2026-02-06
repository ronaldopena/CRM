using System.Data.SqlClient;
using Dapper;
using apimobuss.Entidades;
using apimobuss.Dominio;

namespace apimobuss.Repositorio
{
    public class IntegracaoRepositorio : IIntegracaoRepositorio
    {
        private readonly string _connectionString;

        public IntegracaoRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IntegracaoResposta integracao(DateTime dataUltimaIntegracao)
        {
            var empreendimentosResult = listarEmpreendimentos(dataUltimaIntegracao);
            var blocosResult = listarBlocos(dataUltimaIntegracao);
            var unidadesResult = listarUnidades(dataUltimaIntegracao);
            var clientesResult = listarClientes(dataUltimaIntegracao);

            var result = new IntegracaoResposta()
            {
                empreendimentos = empreendimentosResult,
                blocos = blocosResult,
                unidades = unidadesResult,
                clientes = clientesResult,
            };

            return result;

        }

        public IEnumerable<Empreendimentos> listarEmpreendimentos(DateTime dataUltimaIntegracao)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"exec dbo.CRM_Listar_Empreendimentos_integracao @dataultimaintegracao='{dataUltimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            var result = connection.Query<Empreendimentos>(query);
            return result;
        }

        public IEnumerable<Blocos> listarBlocos(DateTime dataUltimaIntegracao)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"exec dbo.CRM_Listar_Blocos_integracao @dataultimaintegracao='{dataUltimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            var result = connection.Query<Blocos>(query);
            return result;
        }

        public IEnumerable<Unidades> listarUnidades(DateTime dataUltimaIntegracao)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"exec dbo.CRM_Listar_Unidades_integracao @dataultimaintegracao='{dataUltimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            var result = connection.Query<Unidades>(query);
            return result;
        }

        public IEnumerable<Clientes> listarClientes(DateTime dataUltimaIntegracao)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"exec dbo.CRM_Listar_Clientes_integracao @dataultimaintegracao='{dataUltimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            var result = connection.Query<Clientes>(query);
            return result;
        }


        public Boolean TokenValido(string token)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = string.Format("select TokenApiCrmMobuss from OPE_PARAMETRO where CD_BancoDados=1 and CD_Mandante=1 ");
            var result = connection.Query<string>(query).First();
            return (token == result);
        }

    }
}
