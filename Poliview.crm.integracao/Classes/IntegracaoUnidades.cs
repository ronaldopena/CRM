using Dapper;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;

namespace poliview.crm.integracao
{
    public class IntegracaoUnidades : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "UNIDADES";
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private Microsoft.Data.SqlClient.SqlConnection _connectionMSSQL;
        private int totalregistros = 0;
        private int registroatual = 0;
        private string? _cliente;
        private static string _tituloMensagem = "Integração de UNIDADES";
        private IConfiguration _configuration;


        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoUnidades(DateTime? DataHoraUltimaIntegracao,
                                  DateTime DataHoraAtual,
                                  int CodigoTabela,
                                  string connectionStringMssql,
                                  string connectionStringFb,
                                  LogService logService,
                                  IConfiguration configuration)
        {
            _logService = logService;
            _DataHoraUlimaIntegracao = (DateTime)(DataHoraDaUltimaIntegracao == null ? DataHoraDaUltimaIntegracao() : DataHoraUltimaIntegracao);
            _connectionStringFb = connectionStringFb;
            _connectionStringMssql = connectionStringMssql;
            _CodigoTabela = CodigoTabela;
            _DataHoraAtual = DataHoraAtual;
            _connectionFB = new FirebirdConnectionFactory(_connectionStringFb).CreateConnection();
            _connectionMSSQL = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            _cliente = configuration["cliente"] ?? "não identificado";
            _configuration = configuration;
        }

        public bool Integrar()
        {
            try
            {
                Console.WriteLine($"INTEGRANDO {TABELA}");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: inicio de integração {_DataHoraUlimaIntegracao.ToString("dd/MM/yyyy HH:mm:ss")}").Wait();
                var registrosOrigem = this.ListarOrigem();
                totalregistros = registrosOrigem.Count;
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: foram encontrados {registrosOrigem.Count} registros para integração").Wait();
                if (registrosOrigem != null)
                {
                    ExcluirRegistros();
                    salvarDadosDestino(registrosOrigem);
                }
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: fim de integração").Wait();
                // AlterarDataHoraDaUltimaIntegracao();

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: Tempo decorrido: {elapsedTime} ").Wait(); Console.WriteLine();
                return true;
            }
            catch (Exception ex)
            {
                var mensagemErro = $"INTEGRANDO {TABELA}\n\n" +
                    $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                $"Detalhes: {ex.Message}\n\n" +
                (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message} ").Wait(); Console.WriteLine();
                return false;
            }
            finally
            {
                _connectionFB.Close();
                _connectionFB.Dispose();
                _connectionMSSQL.Close();
                _connectionMSSQL.Dispose();
            }
        }

        private bool ExcluirRegistros()
        {
            try
            {
                var registrosOrigemExclusao = this.ListarOrigemExclusao();
                IntegracaoUtil.DeletarOrigemExclusaoNulo(_connectionFB);
                if (registrosOrigemExclusao != null)
                {
                    foreach (var item in registrosOrigemExclusao)
                    {
                        var chave = item.chave.Split(";");
                        try
                        {
                            ExcluirRegistro(chave[0], chave[1], chave[2]);
                            ExcluirRegistroProcessado(item.chave);
                        }
                        catch (Exception ex)
                        {
                            var mensagemErro = $"EXCLUINDO REGISTROS item {TABELA}\n\n" +
                                $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                           $"Detalhes: {ex.Message}\n\n" +
                            (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                           Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(item);
                            IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: Exclusão de registros {chave}").Wait();
                        }
                    }
                }
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: Exclusão de registros").Wait();
                return true;
            }
            catch (Exception ex)
            {
                var mensagemErro = $"EXCLUINDO REGISTROS {TABELA}\n\n" +
                    $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                               $"Detalhes: {ex.Message}\n\n" +
                               (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message}").Wait();
                return false;
            }

        }

        private void ExcluirRegistro(string empreendimento, string bloco, string unidade)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluindo registro. empreendimento: {empreendimento} bloco: {bloco} unidade: {unidade}").Wait();
            var connection = _connectionMSSQL;
            // var sql = $"DELETE FROM [dbo].[CAD_UNIDADE] WHERE CD_EmpreeSP7={empreendimento} AND CD_BlocoSP7={bloco} AND NR_UnidadeSP7='{unidade}'";
            var sql = $"exec CRM_INTEGRACAO_EXCLUIR_REGISTROS_UNIDADE @Empreendimentosp7 = {empreendimento}, @Blocosp7 = {bloco}, @unidadesp7 = N'{unidade}'";
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: sql: {sql}").Wait();
            connection.ExecuteAsync(sql).Wait();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='UNIDADES' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _connectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='UNIDADES'";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }
        private List<UnidadesIntegracao> ListarOrigem()
        {
            var connection = _connectionFB;
            var sql = "SELECT " +
                            "UNDEMPRD_EMPRD as codigoempreendimentosp7, UNDEMPRD_BLOCO as codigoblocosp7, UNDEMPRD_TIPO as tipo, " +
                            "UNDEMPRD_CDG as codigounidadesp7, UNDEMPRD_END as endereco, UNDEMPRD_CALCSTATUSUND as codigostatus, UNDEMPRD_CALCSTATUS as status, " +
                            "UNDEMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM EMP_UNDEMPRD " +
                            $"WHERE UNDEMPRD_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";     
            return connection.Query<UnidadesIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<UnidadesIntegracao> dadosorigem)
        {
            foreach (var proponente in dadosorigem)
            {
                try
                {
                    registroatual += 1;
                    if (JaEstaCadastrado(proponente))
                    {
                        Alterar(proponente);
                    }
                    else
                    {
                        Incluir(proponente);
                    }
                }
                catch (Exception ex)
                {
                    var mensagemErro = $"SALVANDO REGISTROS {TABELA}\n\n" +
                        $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                    $"Detalhes: {ex.Message}\n\n" +
                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                    Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(proponente);

                    IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                    _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message} ").Wait();
                }
            }
        }

        private void Incluir(UnidadesIntegracao obj)
        {

            try
            {
                var connection = _connectionMSSQL;
                var sql = $"EXEC dbo.CRM_INCLUIR_CAD_UNIDADE @empreendimentosp7={obj.codigoempreendimentosp7}, " + 
                          $"@blocosp7={obj.codigoblocosp7}, " +
                          $"@unidadesp7='{obj.codigounidadesp7}', " + 
                          $"@idstatussp7={obj.codigostatus}, @statussp7='{obj.status}', @tipo={obj.tipo} ";               
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro. unidade {obj.codigounidadesp7} bloco {obj.codigoblocosp7} emprd {obj.codigoempreendimentosp7}").Wait();
                connection.ExecuteAsync(sql).Wait();
            }
            catch (Exception ex)
            {
                var mensagemErro = $"INCLUINDO REGISTROS {TABELA}\n\n" +
                    $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                $"Detalhes: {ex.Message}\n\n" +
                (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(obj);
                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} unidade {obj.codigounidadesp7} bloco {obj.codigoblocosp7} emprd {obj.codigoempreendimentosp7}").Wait();
            }
        }

        private void Alterar(UnidadesIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
                var sql = $"EXEC dbo.CRM_ALTERAR_CAD_UNIDADE @empreendimentosp7={obj.codigoempreendimentosp7}, " +
                          $"@blocosp7={obj.codigoblocosp7}, " +
                          $"@unidadesp7='{obj.codigounidadesp7}', " +
                          $"@idstatussp7={obj.codigostatus}, @statussp7='{obj.status}', @tipo={obj.tipo} ";
                /*
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
                        $"      ,[tipo] = '{obj.tipo}' " +
                        $" WHERE CD_EmpreeSP7={obj.codigoempreendimentosp7} and CD_BlocoSP7={obj.codigoblocosp7} and NR_UnidadeSP7='{obj.codigounidadesp7}' ";
                */
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterando registro. unidade {obj.codigounidadesp7} bloco {obj.codigoblocosp7} emprd {obj.codigoempreendimentosp7}").Wait();
                connection.ExecuteAsync(sql).Wait();
            }
            catch (Exception ex)
            {
                var mensagemErro = $"ALTERANDO REGISTROS {TABELA}\n\n" +
                    $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                    $"Detalhes: {ex.Message}\n\n" +
                    (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                    Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(obj);

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} unidade {obj.codigounidadesp7} bloco {obj.codigoblocosp7} emprd {obj.codigoempreendimentosp7}").Wait();
            }
        }

        private Boolean JaEstaCadastrado(UnidadesIntegracao obj)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"select 1 from CAD_UNIDADE where CD_EmpreeSP7={obj.codigoempreendimentosp7} and CD_BlocoSP7={obj.codigoblocosp7} and NR_UnidadeSP7='{obj.codigounidadesp7}'";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }
        private DateTime DataHoraDaUltimaIntegracao()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }
        private void AlterarDataHoraDaUltimaIntegracao()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"UPDATE CAD_INTEGRACAO SET integrar=0, DataUltimaIntegracao='{_DataHoraAtual.ToString("yyyy-MM-dd HH:mm:ss")}' where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            connection.ExecuteAsync(sql).Wait();
        }

    }
}
