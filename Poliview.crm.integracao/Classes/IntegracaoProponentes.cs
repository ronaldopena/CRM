using Dapper;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;

namespace poliview.crm.integracao
{
    public class IntegracaoProponentes : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "PROPONENTES";
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private Microsoft.Data.SqlClient.SqlConnection _connectionMSSQL;
        private int totalregistros = 0;
        private int registroatual = 0;
        private string? _cliente;
        private static string _tituloMensagem = "Integração de PROPONENTES";
        private IConfiguration _configuration;

        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoProponentes(DateTime? DataHoraUltimaIntegracao,
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
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: fim de integração ").Wait();
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
                            ExcluirRegistro(chave[0], chave[1]);
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
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: fim de integração").Wait();
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

        private void ExcluirRegistro(string cliente, string contrato)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_PROPONENTE] WHERE CD_ClienteSP7='{cliente}' AND CD_ContratoSP7='{contrato}'";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro. cliente: {cliente} contrato: {contrato}").Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _connectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='PROPONENTES'";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='PROPONENTES' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private List<ProponenteIntegracao> ListarOrigem()
        {
            var connection = _connectionFB;
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
                    //"WHERE PROPONENTE_CTR='23 D-1-11'";
                    $"WHERE PROPONENTE_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<ProponenteIntegracao>(sql).ToList();
        }
        
        private void salvarDadosDestino(List<ProponenteIntegracao> dadosorigem)
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
                    AjustaProponentesContrato(proponente.contratosp7);
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

        private void Incluir(ProponenteIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
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
                $"           ,[datahoraultimaalteracao]" +
                $"           ,[integrado])" +
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
                $"           ,@datahoraatualizacao"+
                $"           ,1);";

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

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
                connection.ExecuteAsync(sql, parameters).Wait();
            }
            catch (Exception ex)
            {
                var mensagemErro = $"INCLUINDO REGISTROS {TABELA}\n\n" +
                    $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                $"Detalhes: {ex.Message}\n\n" +
                (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(obj);
                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message}  Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
            }                        
        }

        private void Alterar(ProponenteIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
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
                $"      ,[integrado] = 1" +
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

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterado registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
                connection.ExecuteAsync(sql, parameters).Wait();
            }
            catch (Exception ex)
            {
                var mensagemErro = $"ALTERANDO REGISTROS {TABELA}\n\n" +
                    $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                    $"Detalhes: {ex.Message}\n\n" +
                    (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                    Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(obj);

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
            }            
        }

        private Boolean JaEstaCadastrado(ProponenteIntegracao proponente)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"select * from CAD_PROPONENTE WHERE CD_ContratoSP7='{proponente.contratosp7}' and CD_ClienteSP7='{proponente.codigoclientesp7}'";
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

        private void AjustaProponentesContrato(string contrato)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"exec dbo.CRM_Ajusta_cessao_direitos @contrato='{contrato}' ";
            connection.Execute(sql);
        }

    }
}
