using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class IntegracaoProponenteService
    {
        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        private FbConnection _conectionFB;
        private SqlConnection _conectionMSSQL;
        public IntegracaoProponenteService(string _connectionStringFb, string _connectionStringMssql)
        {           
            _conectionFB = new FirebirdConnectionService(_connectionStringFb).CreateConnection();
            _conectionMSSQL = new SqlServerConnectionService(_connectionStringMssql).CreateConnection();
        }

        public void Integrar(string codigoclientesp7)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();            
            var registrosOrigem = this.ListarOrigem(codigoclientesp7);
            if (registrosOrigem != null)
            {
                ExcluirRegistros();
                salvarDadosDestino(registrosOrigem);
            }
    
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
            _conectionFB.Close();
            _conectionFB.Dispose();
            _conectionMSSQL.Close();
            _conectionMSSQL.Dispose();
        }

        private bool ExcluirRegistros()
        {
            try
            {
                var registrosOrigemExclusao = this.ListarOrigemExclusao();
                if (registrosOrigemExclusao != null)
                {
                    foreach (var item in registrosOrigemExclusao)
                    {
                        var chave = item.chave.Split(";");

                        ExcluirRegistro(chave[0], chave[1]);
                        ExcluirRegistroProcessado(item.chave);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        private void ExcluirRegistro(string cliente, string contrato)
        {
            var connection = _conectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_PROPONENTE] WHERE CD_ClienteSP7='{cliente}' AND CD_ContratoSP7='{contrato}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _conectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='PROPONENTES' and chave is not null ";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _conectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='PROPONENTES' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ProponenteIntegracao> ListarOrigem(string codigocontratosp7)
        {
            var connection = _conectionFB;
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
                    $"WHERE PROPONENTE_FORNECEDOR='{codigocontratosp7}'";
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
                AjustaProponentesContrato(proponente.contratosp7);
            }
        }

        private void Incluir(ProponenteIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"set dateformat ymd; INSERT INTO [dbo].[CAD_PROPONENTE]" +
            $"           ([CD_Proponente]" +
            $"           ,[CD_ContratoSP7]" +
            $"           ,[CD_ClienteSP7]" +
            $"           ,[IN_STATUSINTEGRACAO]" +
            $"           ,[DT_Controle]" +
            $"           ,[HR_Controle]" +
            $"           ,[CD_BancoDados]" +
            $"           ,[CD_Mandante]" +
            $"           ,[Principal]" +
            $"           ,[datacessao]" +
            $"           ,[codigocessao]" +
            $"           ,[clienteatual]" +
            $"           ,[clientenovo]" +
            $"           ,[statussessao]" +
            $"           ,[ativo]" +
            $"           ,[datahoraultimaalteracao])" +
            $"     VALUES" +
            $"           (@codigoproponente" +
            $"           ,@contratosp7" +
            $"           ,@codigoclientesp7" +
            $"           ,@statusintegracao" +
            $"           ,@dataatualizacao " +
            $"           ,@horaatualizacao " +
            $"           ,1" +
            $"           ,1" +
            $"           ,@principal" +
            $"           ,@datacessao" +
            $"           ,@codigocessao" +
            $"           ,@clienteatual" +
            $"           ,@clientenovo" +
            $"           ,@statuscessao" +
            $"           ,@ativo" +
            $"           ,@datahoraatualizacao);";

            var parameters = new
            {
                codigoclientesp7 = obj.codigoclientesp7,
                codigoproponente = obj.codigoproponente,
                contratosp7 = obj.contratosp7,
                principal = obj.principal,
                datacessao = obj.datacessao,
                codigocessao = obj.codigocessao,
                clienteatual = obj.clienteatual,
                clientenovo = obj.clientenovo,
                statuscessao = obj.statuscessao,
                ativo = obj.ativo,
                statusintegracao = obj.statusintegracao,
                datahoraatualizacao = obj.datahoraultimaatualizacao,
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm")
            };
            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private void Alterar(ProponenteIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"set dateformat ymd; UPDATE [dbo].[CAD_PROPONENTE] " +
            $"   SET [CD_Proponente] = @codigoproponente " +
            $"      ,[CD_ContratoSP7] = @contratosp7 " +
            $"      ,[CD_ClienteSP7] = @codigoclientesp7 " +
            $"      ,[IN_STATUSINTEGRACAO] = @statusintegracao " +
            $"      ,[DT_Controle] = @dataatualizacao" +
            $"      ,[HR_Controle] = @horaatualizacao " +
            $"      ,[CD_BancoDados] = 1 " +
            $"      ,[CD_Mandante] = 1 " +
            $"      ,[Principal] = @principal " +
            $"      ,[datacessao] = @datacessao" +
            $"      ,[codigocessao] = @codigocessao" +
            $"      ,[clienteatual] = @clienteatual" +
            $"      ,[clientenovo] = @clientenovo" +
            $"      ,[statussessao] = @statuscessao" +
            $"      ,[ativo] = @ativo" +
            $"      ,[datahoraultimaalteracao] = @datahoraautalizacao" +
            $" WHERE CD_ContratoSP7=@contratosp7 and  CD_ClienteSP7=@codigoclientesp7;";

            var parameters = new
            {
                codigoclientesp7 = obj.codigoclientesp7,
                codigoproponente = obj.codigoproponente,
                contratosp7 = obj.contratosp7,
                principal = obj.principal,
                datacessao = obj.datacessao,
                codigocessao = obj.codigocessao,
                clienteatual = obj.clienteatual,
                clientenovo = obj.clientenovo,
                statuscessao = obj.statuscessao,
                statusintegracao = obj.statusintegracao,
                ativo = obj.ativo,
                datahoraautalizacao = obj.datahoraultimaatualizacao.ToString("yyyy-MM-dd HH:mm:ss"),
                dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm")
            };
            connection.ExecuteAsync(sql, parameters).Wait();
        }

        private Boolean JaEstaCadastrado(ProponenteIntegracao proponente)
        {
            var connection = _conectionMSSQL;
            var sql = $"select * from CAD_PROPONENTE WHERE CD_ContratoSP7='{proponente.contratosp7}' and CD_ClienteSP7='{proponente.codigoclientesp7}'";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }
        private DateTime DataHoraDaUltimaIntegracao()
        {
            var connection = _conectionMSSQL;
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela=56 AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }

        private void AjustaProponentesContrato(string contrato)
        {
            var connection = _conectionMSSQL;
            var sql = $"exec dbo.CRM_Ajusta_cessao_direitos @contrato='{contrato}' ";
            connection.Execute(sql);
        }

        private string retornaString(string str, int tamanho)
        {
            if (str == null) return "";

            str = str.Replace("'", "''");

            if (str.Length > tamanho) return str.Substring(0, tamanho);

            return str;
        }
    }
}
