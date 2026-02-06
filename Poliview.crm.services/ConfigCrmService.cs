using Microsoft.Extensions.Configuration;
using Poliview.crm.domain;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Poliview.crm.services
{
    public class ConfigCrmService : IConfigCrmService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public ConfigCrmService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public ConfigCrm getConfigCrm()
        {
            using var connection = new SqlConnection(_connectionString);

            var query = "select * from OPE_CONFIG ";

            var result = connection.QueryFirst<ConfigCrm>(query);

            return result;
        }

        public MenusPermissao getMenus(int idempreendimento)
        {
            using var connection = new SqlConnection(_connectionString);
            /*
            var query = "select boleto, FichaFinanceira, InformeRendimento, Chamado from cad_empreendimento, " +
                        "(select habilitarEspacoCliente from OPE_PARAMETRO where CD_BancoDados=1 and CD_Mandante=1) as habilitarEspacoCliente, " + 
                        "(select empreendimentoTesteEspacoCliente from OPE_PARAMETRO where CD_BancoDados=1 and CD_Mandante=1) as empreendimentoTesteEspacoCliente " +
                        $"where CD_Empreendimento={idempreendimento} ";
            */
            var query = $"exec dbo.CRM_Listar_menus_app  @idempreendimento={idempreendimento}";
            var result = connection.QueryFirst<MenusPermissao>(query);

            // return Json(result, new JsonSerializerOptions { PropertyNamingPolicy = null });

            return result;
        }
    }
}
