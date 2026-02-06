using Dapper;
using FirebirdSql.Data.FirebirdClient;
using Poliview.crm.domain;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class IntegracaoUnidadeService
    {
        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        private FbConnection _conectionFB;
        private SqlConnection _conectionMSSQL;
        private int _CodigoTabela = 5;

        public IntegracaoUnidadeService(string _connectionStringFb, string _connectionStringMssql)
        {           
            _conectionFB = new FirebirdConnectionService(_connectionStringFb).CreateConnection();
            _conectionMSSQL = new SqlServerConnectionService(_connectionStringMssql).CreateConnection();
        }

        public void Integrar(int codigoempreendimentosp7, int codigoblocosp7, string codigounidadesp7)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();            
            var registrosOrigem = this.ListarOrigem(codigoempreendimentosp7, codigoblocosp7, codigounidadesp7);
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
            try
            {
                var registrosOrigemExclusao = this.ListarOrigemExclusao();
                if (registrosOrigemExclusao != null)
                {
                    foreach (var item in registrosOrigemExclusao)
                    {
                        var chave = item.chave.Split(";");
                        ExcluirRegistro(chave[0], chave[1], chave[2]);
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

        private void ExcluirRegistro(string empreendimento, string bloco, string unidade)
        {
            var connection = _conectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_UNIDADE] WHERE AND CD_EmpreeSP7={empreendimento} AND CD_BlocoSP7={bloco} AND NR_UnidadeSP7='{unidade}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _conectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='UNIDADES' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _conectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='UNIDADES' and chave is not null ";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private List<UnidadesIntegracao> ListarOrigem(int codigoempreendimentosp7, int codigoblocosp7, string codigounidadesp7)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                            "UNDEMPRD_EMPRD as codigoempreendimentosp7, UNDEMPRD_BLOCO as codigoblocosp7, " +
                            "UNDEMPRD_CDG as codigounidadesp7, UNDEMPRD_END as endereco, UNDEMPRD_CALCSTATUSUND as codigostatus, UNDEMPRD_CALCSTATUS as status, " +
                            "UNDEMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM EMP_UNDEMPRD " +
                            $"WHERE UNDEMPRD_EMPRD={codigoempreendimentosp7} and UNDEMPRD_BLOCO={codigoblocosp7} and UNDEMPRD_CDG='{codigounidadesp7}'";
            return connection.Query<UnidadesIntegracao>(sql).ToList();
        }

        private List<UnidadesIntegracao> ListarOrigem(DateTime _DataHoraUlimaIntegracao)
        {
            var connection = _conectionFB;
            var sql = "SELECT " +
                            "UNDEMPRD_EMPRD as codigoempreendimentosp7, UNDEMPRD_BLOCO as codigoblocosp7, " +
                            "UNDEMPRD_CDG as codigounidadesp7, UNDEMPRD_END as endereco, UNDEMPRD_CALCSTATUSUND as codigostatus, UNDEMPRD_CALCSTATUS as status, " +
                            "UNDEMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM EMP_UNDEMPRD " +
                            $"WHERE UNDEMPRD_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<UnidadesIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<UnidadesIntegracao> dadosorigem)
        {
            foreach (var unidade in dadosorigem)
            {
                if (JaEstaCadastrado(unidade))
                {
                    Alterar(unidade);
                }
                else
                {
                    Incluir(unidade);
                }
                AjustaOpeUnidade(unidade.codigoempreendimentosp7, unidade.codigoblocosp7, unidade.codigounidadesp7);
            }
        }

        private int ProximoCodigo()
        {
            var connection = _conectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_Unidade)+1,1) as codigo FROM [dbo].[CAD_UNIDADE]";
            var proximo = connection.Query<int>(sql).ToList();
            return proximo[0];
        }

        private void Incluir(UnidadesIntegracao obj)
        {
            var codigo = ProximoCodigo();
            var connection = _conectionMSSQL;
            var sql = "" +
                        $"INSERT INTO [dbo].[CAD_UNIDADE] " +
                        $"           ([CD_BancoDados] " +
                        $"           ,[CD_Mandante] " +
                        $"           ,[CD_Unidade] " +
                        $"           ,[CD_EmpreeSP7] " +
                        $"           ,[CD_BlocoSP7] " +
                        $"           ,[NR_UnidadeSP7] " +
                        $"           ,[DS_Endereco] " +
                        $"           ,[DT_Controle] " +
                        $"           ,[HR_Controle] " +
                        $"           ,[IN_StatusSP7] " +
                        $"           ,[status]) " +
                        $"     VALUES " +
                        $"           (1 " +
                        $"           ,1 " +
                        $"           ,{codigo} " +
                        $"           ,{obj.codigoempreendimentosp7} " +
                        $"           ,{obj.codigoblocosp7} " +
                        $"           ,'{obj.codigounidadesp7}' " +
                        $"           ,'{obj.endereco}' " +
                        $"           ,'{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
                        $"           ,'{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
                        $"           ,{obj.codigostatus} " +
                        $"           ,'{obj.status}') ";
            connection.ExecuteAsync(sql).Wait();
        }

        private void Alterar(UnidadesIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = "" +
                    $"UPDATE [dbo].[CAD_UNIDADE] " +
                    $"   SET [CD_EmpreeSP7] = {obj.codigoempreendimentosp7} " +
                    $"      ,[CD_BlocoSP7] = {obj.codigoblocosp7} " +
                    $"      ,[NR_UnidadeSP7] = '{obj.codigounidadesp7}' " +
                    $"      ,[DS_Endereco] = '{obj.endereco}' " +
                    $"      ,[DT_Controle] = '{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
                    $"      ,[HR_Controle] = '{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
                    $"      ,[IN_StatusSP7] = {obj.codigostatus} " +
                    $"      ,[status] = '{obj.status}' " +
                    $" WHERE CD_EmpreeSP7={obj.codigoempreendimentosp7} and CD_BlocoSP7={obj.codigoblocosp7} and NR_UnidadeSP7='{obj.codigounidadesp7}' ";
            connection.ExecuteAsync(sql).Wait();
        }

        private Boolean JaEstaCadastrado(UnidadesIntegracao obj)
        {
            var connection = _conectionMSSQL;
            var sql = $"select 1 from CAD_UNIDADE where CD_EmpreeSP7={obj.codigoempreendimentosp7} and CD_BlocoSP7={obj.codigoblocosp7} and NR_UnidadeSP7='{obj.codigounidadesp7}'";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
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

        private void AjustaOpeUnidade(int codigoempreendimentosp7, int codigoblocosp7, string codigounidadesp7)
        {
            var connection = _conectionMSSQL;
            var sql = $"exec dbo.CRM_Ajusta_OPE_UNIDADE @Empreendimentosp7={codigoempreendimentosp7}, @blocosp7={codigoblocosp7}, @unidadesp7='{codigounidadesp7}' ";
            connection.Execute(sql);
        }


    }
}
