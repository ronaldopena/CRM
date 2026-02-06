using System.Data.SqlClient;
using Dapper;
using IntegracaoMobussService.dominio;

namespace IntegracaoMobussService.repositorio
{
    public class ConfiguracaoCrmRepositorio
    {
        private readonly string _connectionString;

        public ConfiguracaoCrmRepositorio(string connectioString)
        {           
            _connectionString = connectioString;
        }
    
        public ConfiguracaoCrm Config()
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "SELECT integracaoMobuss, " +
                        "idTipoOcorrenciaMobuss as TipoOcorrenciaMobuss,  " +
                        "idStatusEncerramentoMobuss as StatusEncerramentoMobuss, " +
                        "UrlApiMobuss, " +
                        "TokenApiCrmMobuss, " +
                        "TokenApiMobuss, idOcorrenciaRaizIntegracaoMobuss " +
                        "FROM OPE_PARAMETRO " +
                        "WHERE CD_BANCODADOS = 1 AND CD_MANDANTE = 1 ";

            var result = connection.Query<ConfiguracaoCrm>(query).First();

            return result;

        }
    }
}
