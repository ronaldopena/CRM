using Dapper;
using Microsoft.Extensions.Configuration;
using Poliview.crm.repositorios;

namespace Poliview.crm.services
{
    public class IntegracaoService
    {
        private string _connectionStringMssql;
        private Microsoft.Data.SqlClient.SqlConnection _conectionMSSQL;

        public class TabelaIntegracao
        {
            public int codigotabela { get; set; }
            public string? nometabela { get; set; }
            public DateTime dataultimaintegracao { get; set; }
        }

        public class ConfigIntegracao
        {
            public DateTime DataUltimaIntegracao { get; set; }
            public int integrando { get; set; }
        }

        public IntegracaoService(IConfiguration configuration)
        {
            _connectionStringMssql = configuration["conexao"];
            _conectionMSSQL = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
        }

        public List<TabelaIntegracao> listarTabelasParaIntegracao()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;
            var sql = "select cd_tabela as codigotabela, NM_Integracao as nometabela, DataUltimaIntegracao as dataultimaintegracao from CAD_INTEGRACAO  " +
                      "where CD_BancoDados = 1 and CD_Mandante = 1 and integrar = 1 ";
            var retorno = connection.Query<TabelaIntegracao>(sql).ToList();
            return retorno;
        }

        public ConfigIntegracao config()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;
            var sql = "select top 1 * from OPE_INTEGRACAO  ";
            var retorno = connection.QueryFirst<ConfigIntegracao>(sql);
            return retorno;        
        }

        public Boolean Inicio()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;

            var sql = "select integrando from OPE_INTEGRACAO  ";
            var retorno = connection.QueryFirst<int>(sql);
            
            if (retorno == 0)
            {
                sql = "update OPE_INTEGRACAO SET integrando=1 ";
                connection.Execute(sql);
            }

            return (retorno==0);

        }

        public void Fim()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;
            var sql = "update OPE_INTEGRACAO SET integrando=0 ";
            connection.Execute(sql);
        }

        public void AlterarDataHoraUltimaIntegracao(DateTime data)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;
            var sql = $"set dateformat ymd; update OPE_INTEGRACAO SET DataUltimaIntegracao='{data.ToString("yyyy-MM-dd HH:mm:ss")}', integrando=0; ";
            Console.WriteLine(sql);
            connection.Execute(sql);
        }

        public void AjustaProponentes()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;
            var sql = $"exec dbo.CRM_Ajusta_Proponentes ";
            connection.Execute(sql);
        }

        public void AjustaProponentesContrato(string contrato )
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;
            var sql = $"exec dbo.CRM_Ajusta_cessao_direitos @contrato='{contrato}' ";
            connection.Execute(sql);
        }

    }
}
