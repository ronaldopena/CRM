using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class IntegracaoContratoService
    {
        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        private FbConnection _conectionFB;
        private SqlConnection _conectionMSSQL;
        private int _CodigoTabela = 2;   
        public IntegracaoContratoService(string _connectionStringFb, string _connectionStringMssql)
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
            _conectionMSSQL.Close();
        }

        private bool ExcluirRegistros()
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

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            // var connection = _conectionFB;
            var connection = _conectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='contratos' and chave is not null ";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private void ExcluirRegistro(string cliente, string contrato)
        {
            var connection = _conectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_CONTRATO] WHERE CD_ContratoSP7='{contrato}' AND CD_ClienteSP7='{cliente}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _conectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='CONTRATOS' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
        }

        public List<ContratoIntegracao> ListarOrigem(string codigoclientesp7)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                        "CTR_CDG AS contratosp7, CTR_FORNECEDOR as codigoclientesp7, CTR_EMPRE as codigoempresasp7, " +
                        "CTR_EMPRD as codigoempreendimentosp7, CTR_BLOCO as codigoblocosp7, CTR_UNDEMPRD as codigounidadesp7, " +
                        "CTR_STATUS as statuscontratosp7, CTR_STATUSDISTRATO as statusdistrato, CTR_REMANEJADO as statusremanejado, " +
                        "CTR_CNPJCPFRESP as cpfresponsavel, CTR_NOMERESP as nomeresponsavel, CTR_TELEFONERESP as telefoneresponsavel, " +
                        "CTR_CELULARRESP as celularresponsavel, CTR_EMAILRESP as emailresponsavel, CTR_DTLIDOCRM as datahoraultimaatualizacao " +
                        "FROM EMP_CTR " +
                        $"WHERE CTR_FORNECEDOR='{codigoclientesp7}'";
            return connection.Query<ContratoIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<ContratoIntegracao> dadosorigem)
        {
            var x = 0;
            foreach (var item in dadosorigem)
            {
                if (JaEstaCadastrado(item))
                {
                    Alterar(item);
                }
                else
                {
                    Incluir(item);
                }
                AjustaProponentesContrato(item.contratosp7);
                AjustaOpeContrato(item.contratosp7);
            }
        }

        private int ProximoCodigo()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _conectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_Contrato)+1,1) as codigo FROM [dbo].[CAD_CONTRATO]";
            var proximo = connection.Query<int>(sql).ToList();
            return proximo[0];
        }
        private void Incluir(ContratoIntegracao obj)
        {
            var codigo = ProximoCodigo();
            var connection = _conectionMSSQL;
            var sql = "INSERT INTO [dbo].[CAD_CONTRATO] " +
            "           ([CD_BancoDados] " +
            "           ,[CD_Mandante] " +
            "           ,[CD_Contrato] " +
            "           ,[CD_ContratoSP7] " +
            "           ,[CD_ClienteSP7] " +
            "           ,[CD_EmpresaSP7] " +
            "           ,[CD_EmpreeSP7] " +
            "           ,[CD_BlocoSP7] " +
            "           ,[NR_UnidadeSP7] " +
            "           ,[IN_StatusSP7] " +
            "           ,[IN_StatusDistratoSP7] " +
            "           ,[IN_StatusRemanejadoSP7] " +
            "           ,[DT_Controle] " +
            "           ,[HR_Controle] " +
            "           ,[In_StatusIntegracao] " +
            "           ,[CTR_CNPJCPFRESP] " +
            "           ,[CTR_NOMERESP] " +
            "           ,[CTR_TELEFONERESP] " +
            "           ,[CTR_CELULARRESP] " +
            "           ,[CTR_EMAILRESP] " +
            "           ,[DTVENDARESP]) " +
            "     VALUES " +
            "           (1 " +
            "           ,1 " +
            $"           ,{codigo} " +
            $"           ,'{obj.contratosp7}' " +
            $"           ,'{obj.codigoclientesp7}'" +
            $"           ,{obj.codigoempresasp7} " +
            $"           ,{obj.codigoempreendimentosp7} " +
            $"           ,{obj.codigoblocosp7} " +
            $"           ,'{obj.codigounidadesp7}' " +
            $"           ,{obj.statuscontratosp7} " +
            $"           ,{obj.statusdistrato}" +
            $"           ,{obj.statusremanejado} " +
            $"           ,'{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
            $"           ,'{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
            $"           ,0 " +
            $"           ,'{obj.cpfresponsavel}'" +
            $"           ,'{obj.nomeresponsavel}'" +
            $"           ,'{obj.telefoneresponsavel}'" +
            $"           ,'{obj.celularresponsavel}'" +
            $"           ,'{obj.emailresponsavel}' " +
            $"           ,null) ";
            connection.ExecuteAsync(sql).Wait();
        }

        private void Alterar(ContratoIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"UPDATE [dbo].[CAD_CONTRATO] " +
                    $"   SET [CD_BancoDados] = 1 " +
                    $"      ,[CD_Mandante] = 1 " +
                    $"      ,[CD_ContratoSP7] = '{obj.contratosp7}' " +
                    $"      ,[CD_ClienteSP7] = '{obj.codigoclientesp7}' " +
                    $"      ,[CD_EmpresaSP7] = {obj.codigoempresasp7} " +
                    $"      ,[CD_EmpreeSP7] = {obj.codigoempreendimentosp7} " +
                    $"      ,[CD_BlocoSP7] = {obj.codigoblocosp7} " +
                    $"      ,[NR_UnidadeSP7] = '{obj.codigounidadesp7}' " +
                    $"      ,[IN_StatusSP7] = {obj.statuscontratosp7} " +
                    $"      ,[IN_StatusDistratoSP7] = {obj.statusdistrato} " +
                    $"      ,[IN_StatusRemanejadoSP7] = {obj.statusremanejado} " +
                    $"      ,[DT_Controle] = '{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
                    $"      ,[HR_Controle] = '{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
                    $"      ,[In_StatusIntegracao] = '{obj.statusintegracao}' " +
                    $"      ,[CTR_CNPJCPFRESP] = '{obj.cpfresponsavel}' " +
                    $"      ,[CTR_NOMERESP] = '{obj.nomeresponsavel}' " +
                    $"      ,[CTR_TELEFONERESP] = '{obj.telefoneresponsavel}' " +
                    $"      ,[CTR_CELULARRESP] = '{obj.celularresponsavel}' " +
                    $"      ,[CTR_EMAILRESP] = '{obj.emailresponsavel}' " +
                    $"      ,[DTVENDARESP] = null " +
                    $" WHERE CD_ContratoSP7='{obj.contratosp7}' ";
            connection.ExecuteAsync(sql).Wait();
        }

        private Boolean JaEstaCadastrado(ContratoIntegracao contrato)
        {
            var connection = _conectionMSSQL;
            var sql = $"select * from CAD_CONTRATO WHERE CD_ContratoSP7='{contrato.contratosp7}'";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }

        private void AlterarDataHoraUltimaAlteracaoUnidade(ContratoIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"update CAD_UNIDADE " +
                      $"SET DT_CONTROLE='{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}', HR_CONTROLE='{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
                      $"WHERE CD_BancoDados=1 AND CD_Mandante=1 and CD_EmpreeSP7={obj.codigoempreendimentosp7} " +
                      $"and CD_BlocoSP7={obj.codigoblocosp7} and NR_UnidadeSP7='{obj.codigounidadesp7}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private void AjustaCessaoDireitosContrato(ContratoIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"exec CRM_Ajusta_cessao_direitos @contrato='{obj.contratosp7}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private void AjustaProponentesContrato()
        {
            var connection = _conectionMSSQL;
            var sql = $"exec CRM_Ajusta_Proponentes";
            connection.ExecuteAsync(sql).Wait();
        }

        private DateTime DataHoraDaUltimaIntegracao()
        {
            var connection = _conectionMSSQL;
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }
        private void AlterarDataHoraDaUltimaIntegracao(DateTime _DataHoraAtual)
        {
            var connection = _conectionMSSQL;
            var sql = $"UPDATE CAD_INTEGRACAO SET integrar=0, DataUltimaIntegracao='{_DataHoraAtual.ToString("yyyy-MM-dd HH:mm:ss")}' where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            connection.ExecuteAsync(sql).Wait();
        }

        private void AjustaProponentesContrato(string contrato)
        {
            var connection = _conectionMSSQL;
            var sql = $"exec dbo.CRM_Ajusta_cessao_direitos @contrato='{contrato}' ";
            connection.Execute(sql);
        }
        private void AjustaOpeContrato(string contratosp7)
        {
            var connection = _conectionMSSQL;
            var sql = $"exec dbo.CRM_ajusta_ope_contrato @contrato='{contratosp7}' ";
            connection.Execute(sql);
        }
    }
}
