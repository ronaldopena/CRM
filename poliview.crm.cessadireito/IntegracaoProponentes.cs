using Dapper;
using poliview.crm.cessadireito.Repositorios;
using poliview.crm.cessadireito.Services;
using Poliview.crm.domain;

namespace poliview.crm.cessadireito
{
    public class IntegracaoProponentes : IIntegracao
    {
        private DateTime _DataHoraUlimaIntegracao;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;

        public IntegracaoProponentes(DateTime DataHoraUltimaIntegracao,
                                     string connectionStringMssql,
                                     string connectionStringFb,
                                     LogService logService)
        {
            _logService = logService;
            _DataHoraUlimaIntegracao = DataHoraUltimaIntegracao;
            _connectionStringFb = connectionStringFb;
            _connectionStringMssql = connectionStringMssql;
        }
        public bool Integrar()
        {
            _logService.Log(LogLevel.Information, $"PROPONENTES: inicio de integração {_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}").Wait();
            var registrosOrigem = this.ListarOrigem();
            _logService.Log(LogLevel.Information, $"PROPONENTE: foram encontrados {registrosOrigem.Count} registro para integração").Wait();
            if (registrosOrigem != null)
            {
                salvarDadosDestino(registrosOrigem);
            }
            _logService.Log(LogLevel.Information, "PROPONENTES: fim de integração").Wait();
            return true;
        }

        private List<ProponenteIntegracao> ListarOrigem()
        {
            var connection = new FirebirdConnectionFactory(_connectionStringFb).CreateConnection();
            var sql = "SELECT " +
                    "PROPONENTE_CDG as codigoproponente, " +
                    "PROPONENTE_CTR as contratosp7, " +
                    "PROPONENTE_FORNECEDOR as codigoclientesp7, " +
                    "CASE EMP_CTR.ctr_cdg when PROPONENTE_CTR THEN 'S' ELSE 'N' end as principal, " +
                    "coalesce(crb_cessao.cessao_dt,'1900-01-01 00:00:00') as datacessao, " +
                    "coalesce(crb_cessao.cessao_cdg,0) as codigocessao, " +
                    "coalesce(crb_cessao.cessao_fornecedoratual,'') as clienteatual, " +
                    "coalesce(crb_cessao.cessao_fornecedor,'') as clientenovo, " +
                    "coalesce(crb_cessao.cessao_status,0) as statuscessao, 'N' as ativo,  emp_proponente.proponente_dtlidocrm as datahoraultimaatualizacao " +
                    "FROM EMP_PROPONENTE " +
                    "left JOIN emp_ctr ON EMP_CTR.ctr_fornecedor = EMP_PROPONENTE.proponente_fornecedor AND emp_ctr.ctr_cdg = EMP_PROPONENTE.proponente_ctr " +
                    "left join crb_cessao on cessao_cdg = proponente_cessao " +
                    $"WHERE PROPONENTE_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<ProponenteIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<ProponenteIntegracao> dadosorigem)
        {
            foreach (var proponente in dadosorigem)
            {
                if (JaEstaCadastrado(proponente))
                {
                    Alterar(proponente);
                }
                else
                {
                    Incluir(proponente);
                }
            }
        }

        private void Incluir(ProponenteIntegracao obj)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = "INSERT INTO [dbo].[CAD_PROPONENTE]" +
                     "           ([CD_Proponente]" +
                     "           ,[CD_ContratoSP7]" +
                     "           ,[CD_ClienteSP7]" +
                     "           ,[IN_STATUSINTEGRACAO]" +
                     "           ,[DT_Controle]" +
                     "           ,[HR_Controle]" +
                     "           ,[CD_BancoDados]" +
                     "           ,[CD_Mandante]" +
                     "           ,[Principal]" +
                     "           ,[datacessao]" +
                     "           ,[codigocessao]" +
                     "           ,[clienteatual]" +
                     "           ,[clientenovo]" +
                     "           ,[statussessao]" +
                     "           ,[ativo]" +
                     "           ,[datahoraultimaalteracao])" +
                     "     VALUES" +
                     "           (@codigoproponente" +
                     "           ,@contratosp7" +
                     "           ,@codigoclientesp7" +
                     "           ,@statusintegracao" +
                     "           ,@dtcontrole" +
                     "           ,@hrcontrole" +
                     "           ,1" +
                     "           ,1" +
                     "           ,@principal" +
                     "           ,@datacessao" +
                     "           ,@codigocessao" +
                     "           ,@clienteatual" +
                     "           ,@clientenovo" +
                     "           ,@statuscessao" +
                     "           ,@ativo" +
                     "           ,@datahoraultimaalteracao)";

            var parameters = new
            {
                codigoproponente = obj.codigoproponente,
                contratosp7 = obj.contratosp7,
                codigoclientesp7 = obj.codigoclientesp7,
                statusintegracao = "",
                dtcontrole = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                hrcontrole = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                principal = obj.principal,
                datacessao = obj.datacessao?.ToString("yyyy-MM-dd HH:mm:ss"),
                codigocessao = obj.codigocessao,
                clienteatual = obj.clienteatual,
                clientenovo = obj.clientenovo,
                statuscessao = obj.statuscessao,
                ativo = obj.ativo,
                datahoraultimaalteracao = obj.datahoraultimaatualizacao.ToString("yyyy-MM-dd HH:mm:ss")
            };

            connection.ExecuteAsync(sql, parameters).Wait();
            _logService.Log(LogLevel.Information, $"PROPONENTE: incluido registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
        }

        private void Alterar(ProponenteIntegracao obj)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = "UPDATE [dbo].[CAD_PROPONENTE] " +
                     "   SET [CD_Proponente] = @codigoproponente " +
                     "      ,[CD_ContratoSP7] = @contratosp7 " +
                     "      ,[CD_ClienteSP7] = @codigoclientesp7 " +
                     "      ,[IN_STATUSINTEGRACAO] = @statusintegracao " +
                     "      ,[DT_Controle] = @dtcontrole " +
                     "      ,[HR_Controle] = @hrcontrole " +
                     "      ,[CD_BancoDados] = 1 " +
                     "      ,[CD_Mandante] = 1 " +
                     "      ,[Principal] = @principal " +
                     "      ,[datacessao] = @datacessao " +
                     "      ,[codigocessao] = @codigocessao " +
                     "      ,[clienteatual] = @clienteatual " +
                     "      ,[clientenovo] = @clientenovo " +
                     "      ,[statussessao] = @statuscessao " +
                     "      ,[ativo] = @ativo " +
                     "      ,[datahoraultimaalteracao] = @datahoraultimaalteracao " +
                     " WHERE CD_ContratoSP7=@contratosp7_where and CD_ClienteSP7=@codigoclientesp7_where";

            var parameters = new
            {
                codigoproponente = obj.codigoproponente,
                contratosp7 = obj.contratosp7,
                codigoclientesp7 = obj.codigoclientesp7,
                statusintegracao = obj.statusintegracao,
                dtcontrole = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                hrcontrole = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                principal = obj.principal,
                datacessao = obj.datacessao?.ToString("yyyy-MM-dd HH:mm:ss"),
                codigocessao = obj.codigocessao,
                clienteatual = obj.clienteatual,
                clientenovo = obj.clientenovo,
                statuscessao = obj.statuscessao,
                ativo = obj.ativo,
                datahoraultimaalteracao = obj.datahoraultimaatualizacao.ToString("yyyy-MM-dd HH:mm:ss"),
                contratosp7_where = obj.contratosp7,
                codigoclientesp7_where = obj.codigoclientesp7
            };

            connection.ExecuteAsync(sql, parameters).Wait();
            _logService.Log(LogLevel.Information, $"PROPONENTE: alterado registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
        }

        private Boolean JaEstaCadastrado(ProponenteIntegracao proponente)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = "select COUNT(*) from CAD_PROPONENTE WHERE CD_ContratoSP7=@contratosp7 and CD_ClienteSP7=@codigoclientesp7";
            var count = connection.QuerySingle<int>(sql, new { contratosp7 = proponente.contratosp7, codigoclientesp7 = proponente.codigoclientesp7 });
            return count > 0;
        }
    }
}
