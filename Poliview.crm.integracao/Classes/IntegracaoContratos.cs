using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;

namespace poliview.crm.integracao
{
    public class IntegracaoContratos : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "CONTRATOS";
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private Microsoft.Data.SqlClient.SqlConnection _connectionMSSQL;
        private int totalregistros = 0;
        private int registroatual = 0;
        private string? _cliente;
        private static string _tituloMensagem = "Integração de CONTRATOS";
        private IConfiguration _configuration;

        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoContratos(DateTime? DataHoraUltimaIntegracao,
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
                CorrigeOpeContrato();
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
                        catch (Exception exe)
                        {
                            var mensagemErro = $"EXCLUINDO REGISTROS {TABELA}\n\n" +
                                 $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                           $"Detalhes: {exe.Message}\n\n" +
                           (exe.InnerException != null ? $"Inner Exception: {exe.InnerException.Message}" : "") + "\n\n" +
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

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            // var connection = _connectionFB;
            var connection = _connectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='contratos'";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private void ExcluirRegistro(string cliente, string contrato)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_CONTRATO] WHERE CD_ContratoSP7='{contrato}' AND CD_ClienteSP7='{cliente}'";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro. cliente: {cliente} Contrato: {contrato}").Wait();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='CONTRATOS' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro processado. tabela: CONTRATOS chave: {chave} ").Wait();
        }

        private List<ContratoIntegracao> ListarOrigem()
        {
            var connection = _connectionFB;
            var sql = $@"SELECT
                        CTR_CDG AS contratosp7, CTR_FORNECEDOR as codigoclientesp7, CTR_EMPRE as codigoempresasp7,
                        CTR_EMPRD as codigoempreendimentosp7, CTR_BLOCO as codigoblocosp7, CTR_UNDEMPRD as codigounidadesp7,
                        CTR_STATUS as statuscontratosp7, CTR_STATUSDISTRATO as statusdistrato, CTR_REMANEJADO as statusremanejado,
                        CTR_CNPJCPFRESP as cpfresponsavel, CTR_NOMERESP as nomeresponsavel, CTR_TELEFONERESP as telefoneresponsavel,
                        CTR_CELULARRESP as celularresponsavel, CTR_EMAILRESP as emailresponsavel, CTR_DTLIDOCRM as datahoraultimaatualizacao
                        FROM EMP_CTR
                        WHERE CTR_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<ContratoIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<ContratoIntegracao> dadosorigem)
        {
            var con = new ContratoIntegracao();
            foreach (var item in dadosorigem)
            {
                con = item;
                registroatual += 1;
                try
                {
                    var codigoContrato = JaEstaCadastrado(item);
                    if (codigoContrato > 0)
                    {
                        item.codigocontrato = codigoContrato;
                        Alterar(item);
                    }
                    else
                    {
                        codigoContrato = IncluirCadContrato(item);
                    }
                    item.codigocontrato = codigoContrato;

                    AjustaOpeContrato(item.contratosp7);
                    AjustaProponentesContrato(item.contratosp7);
                }
                catch (Exception ex)
                {
                    var mensagemErro = $"SALVANDO REGISTROS {TABELA}\n\n" +
                         $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                        $"Detalhes: {ex.Message}\n\n" +
                         (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                        Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(item);

                    IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);

                    Poliview.crm.infra.Util.ExibirPropriedades(con);
                    _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message}").Wait();
                }
            }
        }

        private int ProximoCodigo()
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_Contrato)+1,1) as codigo FROM [dbo].[CAD_CONTRATO] where CD_CONTRATO > 0";
            var proximo = connection.QueryFirst<int>(sql);
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: gerado novo código contrato: {proximo}").Wait();
            return proximo;
        }
        private void CorrigeOpeContrato()
        {
            try
            {
                var connection = _connectionMSSQL;
                var sql = "INSERT INTO OPE_CONTRATO(CD_BancoDados, CD_Mandante, CD_Contrato, CD_Cliente, CD_Empreendimento, CD_Bloco, CD_Unidade, DT_Controle) " +
                "select distinct c.CD_BancoDados, c.CD_Mandante, c.CD_Contrato, cli.CD_Cliente, emp.CD_Empreendimento, blo.CD_Bloco, uni.CD_Unidade, c.DT_Controle " +
                "from CAD_PROPONENTE cp " +
                "left join CAD_CLIENTE cc on cc.CD_ClienteSP7 = cp.CD_ClienteSP7 " +
                "left join CAD_CLIENTE cli on cli.CD_Cliente = cc.CD_Cliente " +
                "left join OPE_CONTRATO oc on oc.CD_Cliente = cli.CD_Cliente " +
                "left join CAD_CONTRATO c on c.CD_ContratoSP7 = cp.CD_ContratoSP7 " +
                "left join CAD_EMPREENDIMENTO emp on emp.CD_EmpreeSP7 = c.CD_EmpreeSP7 " +
                "left join CAD_BLOCO blo on blo.CD_BlocoSP7 = c.CD_BlocoSP7 and blo.CD_EmpreeSP7 = c.CD_EmpreeSP7 " +
                "left join CAD_UNIDADE uni on uni.NR_UnidadeSP7 = c.NR_UnidadeSP7 and uni.CD_EmpreeSP7 = c.CD_EmpreeSP7 and uni.CD_BlocoSP7 = c.CD_BlocoSP7 " +
                "where oc.CD_Cliente is null and cli.CD_Cliente is not null and c.CD_BancoDados is not null ";
                connection.ExecuteAsync(sql).Wait();
            }
            catch (Exception ex)
            {
                var mensagemErro = $"CORRIGINDO OPE_CONTRATO {TABELA}\n\n" +
                     $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                          $"Detalhes: {ex.Message}\n\n" +
                          (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO: {ex.Message} CorrigeOpeContrato").Wait();
            }
        }

        private int IncluirCadContrato(ContratoIntegracao obj)
        {
            try
            {
                var codigo = ProximoCodigo();
                obj.codigocontrato = codigo;
                var connection = _connectionMSSQL;
                var sql = @"INSERT INTO [dbo].[CAD_CONTRATO]
                           ([CD_BancoDados]
                           ,[CD_Mandante]
                           ,[CD_Contrato]
                           ,[CD_ContratoSP7]
                           ,[CD_ClienteSP7]
                           ,[CD_EmpresaSP7]
                           ,[CD_EmpreeSP7]
                           ,[CD_BlocoSP7]
                           ,[NR_UnidadeSP7]
                           ,[IN_StatusSP7]
                           ,[IN_StatusDistratoSP7]
                           ,[IN_StatusRemanejadoSP7]
                           ,[DT_Controle]
                           ,[HR_Controle]
                           ,[In_StatusIntegracao]
                           ,[CTR_CNPJCPFRESP]
                           ,[CTR_NOMERESP]
                           ,[CTR_TELEFONERESP]
                           ,[CTR_CELULARRESP]
                           ,[CTR_EMAILRESP]
                           ,[DTVENDARESP]
                           ,[integrado])
                     VALUES
                           (@CD_BancoDados
                           ,@CD_Mandante
                           ,@CD_Contrato
                           ,@CD_ContratoSP7
                           ,@CD_ClienteSP7
                           ,@CD_EmpresaSP7
                           ,@CD_EmpreeSP7
                           ,@CD_BlocoSP7
                           ,@NR_UnidadeSP7
                           ,@IN_StatusSP7
                           ,@IN_StatusDistratoSP7
                           ,@IN_StatusRemanejadoSP7
                           ,@DT_Controle
                           ,@HR_Controle
                           ,@In_StatusIntegracao
                           ,@CTR_CNPJCPFRESP
                           ,@CTR_NOMERESP
                           ,@CTR_TELEFONERESP
                           ,@CTR_CELULARRESP
                           ,@CTR_EMAILRESP
                           ,@DTVENDARESP
                           ,@integrado)";

                var parametros = new
                {
                    CD_BancoDados = 1,
                    CD_Mandante = 1,
                    CD_Contrato = obj.codigocontrato,
                    CD_ContratoSP7 = obj.contratosp7,
                    CD_ClienteSP7 = obj.codigoclientesp7,
                    CD_EmpresaSP7 = obj.codigoempresasp7,
                    CD_EmpreeSP7 = obj.codigoempreendimentosp7,
                    CD_BlocoSP7 = obj.codigoblocosp7,
                    NR_UnidadeSP7 = obj.codigounidadesp7,
                    IN_StatusSP7 = obj.statuscontratosp7,
                    IN_StatusDistratoSP7 = obj.statusdistrato,
                    IN_StatusRemanejadoSP7 = obj.statusremanejado,
                    DT_Controle = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    HR_Controle = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    In_StatusIntegracao = 0,
                    CTR_CNPJCPFRESP = obj.cpfresponsavel,
                    CTR_NOMERESP = obj.nomeresponsavel,
                    CTR_TELEFONERESP = obj.telefoneresponsavel,
                    CTR_CELULARRESP = obj.celularresponsavel,
                    CTR_EMAILRESP = obj.emailresponsavel,
                    DTVENDARESP = (DateTime?)null,
                    integrado = 1
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
                connection.ExecuteAsync(sql, parametros).Wait();
                return codigo;
            }
            catch (Exception ex)
            {
                var mensagemErro = $"INCLUINDO REGISTROS {TABELA}\n\n" +
                     $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                   $"Detalhes: {ex.Message}\n\n" +
                   (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                   Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(obj);

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO: {ex.Message} Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
                return 0;
            }
        }

        private void Alterar(ContratoIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
                var sql = @"UPDATE [dbo].[CAD_CONTRATO]
                           SET [CD_BancoDados] = 1
                              ,[CD_Mandante] = 1
                              ,[CD_ContratoSP7] = @ContratoSP7
                              ,[CD_ClienteSP7] = @CodigoClienteSP7
                              ,[CD_EmpresaSP7] = @CodigoEmpresaSP7
                              ,[CD_EmpreeSP7] = @CodigoEmpreendimentoSP7
                              ,[CD_BlocoSP7] = @CodigoBlocoSP7
                              ,[NR_UnidadeSP7] = @CodigoUnidadeSP7
                              ,[IN_StatusSP7] = @StatusContratoSP7
                              ,[IN_StatusDistratoSP7] = @StatusDistrato
                              ,[IN_StatusRemanejadoSP7] = @StatusRemanejado
                              ,[DT_Controle] = @DataControle
                              ,[HR_Controle] = @HoraControle
                              ,[In_StatusIntegracao] = @StatusIntegracao
                              ,[CTR_CNPJCPFRESP] = @CpfResponsavel
                              ,[CTR_NOMERESP] = @NomeResponsavel
                              ,[CTR_TELEFONERESP] = @TelefoneResponsavel
                              ,[CTR_CELULARRESP] = @CelularResponsavel
                              ,[CTR_EMAILRESP] = @EmailResponsavel
                              ,[DTVENDARESP] = null
                              ,[integrado] = 1
                           WHERE CD_ContratoSP7 = @ContratoSP7Where";

                var parametros = new
                {
                    ContratoSP7 = obj.contratosp7,
                    CodigoClienteSP7 = obj.codigoclientesp7,
                    CodigoEmpresaSP7 = obj.codigoempresasp7,
                    CodigoEmpreendimentoSP7 = obj.codigoempreendimentosp7,
                    CodigoBlocoSP7 = obj.codigoblocosp7,
                    CodigoUnidadeSP7 = obj.codigounidadesp7,
                    StatusContratoSP7 = obj.statuscontratosp7,
                    StatusDistrato = obj.statusdistrato,
                    StatusRemanejado = obj.statusremanejado,
                    DataControle = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    HoraControle = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    StatusIntegracao = obj.statusintegracao,
                    CpfResponsavel = obj.cpfresponsavel,
                    NomeResponsavel = obj.nomeresponsavel,
                    TelefoneResponsavel = obj.telefoneresponsavel,
                    CelularResponsavel = obj.celularresponsavel,
                    EmailResponsavel = obj.emailresponsavel,
                    ContratoSP7Where = obj.contratosp7
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterando registro. Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
                connection.ExecuteAsync(sql, parametros).Wait();
            }
            catch (Exception ex)
            {
                var mensagemErro = $"ALTERANDO REGISTROS {TABELA}\n\n" +
                     $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                  $"Detalhes: {ex.Message}\n\n" +
                  (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                  Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(obj);

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO: {ex.Message} Proponente: {obj.codigoclientesp7} Contrato: {obj.contratosp7}").Wait();
            }
        }

        private int JaEstaCadastrado(ContratoIntegracao contrato)
        {
            var connection = _connectionMSSQL;
            var sql = $"select TOP 1 CD_CONTRATO from CAD_CONTRATO WHERE CD_ContratoSP7='{contrato.contratosp7}'";
            var ret = connection.QueryFirstOrDefault<int>(sql);
            return (ret);
        }

        private void AlterarDataHoraUltimaAlteracaoUnidade(ContratoIntegracao obj)
        {
            var connection = _connectionMSSQL;
            var sql = $"update CAD_UNIDADE " +
                      $"SET DT_CONTROLE='{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}', HR_CONTROLE='{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
                      $"WHERE CD_BancoDados=1 AND CD_Mandante=1 and CD_EmpreeSP7={obj.codigoempreendimentosp7} " +
                      $"and CD_BlocoSP7={obj.codigoblocosp7} and NR_UnidadeSP7='{obj.codigounidadesp7}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private void AjustaCessaoDireitosContrato(ContratoIntegracao obj)
        {
            var connection = _connectionMSSQL;
            var sql = $"exec CRM_Ajusta_cessao_direitos @contrato='{obj.contratosp7}'";
            connection.ExecuteAsync(sql).Wait();
        }

        private void AjustaProponentesContrato()
        {
            var connection = _connectionMSSQL;
            var sql = $"exec CRM_Ajusta_Proponentes";
            connection.ExecuteAsync(sql).Wait();
        }

        private DateTime DataHoraDaUltimaIntegracao()
        {
            var connection = _connectionMSSQL;
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }
        private void AlterarDataHoraDaUltimaIntegracao()
        {
            var connection = _connectionMSSQL;
            var sql = $"UPDATE CAD_INTEGRACAO SET integrar=0, DataUltimaIntegracao='{_DataHoraAtual.ToString("yyyy-MM-dd HH:mm:ss")}' where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            connection.ExecuteAsync(sql).Wait();
        }

        private void AjustaProponentesContrato(string contrato)
        {
            var connection = _connectionMSSQL;
            var sql = $"exec dbo.CRM_Ajusta_cessao_direitos @contrato='{contrato}' ";
            connection.Execute(sql);
        }
        private void AjustaOpeContrato(string contrato)
        {
            var connection = _connectionMSSQL;
            var sql = $"exec dbo.CRM_ajusta_ope_contrato @contrato='{contrato}' ";
            connection.Execute(sql);
        }

    }
}
