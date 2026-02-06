using Dapper;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;

namespace poliview.crm.integracao
{
    public class IntegracaoEmpreendimentos : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "EMPREENDIMENTOS";
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private Microsoft.Data.SqlClient.SqlConnection _connectionMSSQL;
        private int totalregistros = 0;
        private int registroatual = 0;
        private string? _cliente;
        private static string _tituloMensagem = "Integração de EMPREENDIMENTOS";
        private IConfiguration _configuration;

        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoEmpreendimentos(DateTime? DataHoraUltimaIntegracao,
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
                return false; ;
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
                            ExcluirRegistro(chave[0]);
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

        private void ExcluirRegistro(string empreendimento)
        {
            var connection = _connectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_EMPREENDIMENTO] WHERE AND CD_EmpreeSP7={empreendimento}";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro. empreendimento: {empreendimento}").Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _connectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='EMPREENDIMENTOS'";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where tabela='EMPREENDIMENTOS' AND CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro processado. tabela: EMPREENDIMENTOS chave: {chave} ").Wait();
        }
        private List<EmpreendimentosIntegracao> ListarOrigem()
        {
            var connection = _connectionFB;
            var sql = "SELECT " +
                            "EMPRD_CDG as empreendimentosp7, EMPRD_DESC as empreendimento, EMPRD_ABREV as abreviatura, EMPRD_END as endereco, " +
                            "EMPRD_BAIRRO as bairro, EMPRD_CID as cidade, EMPRD_UF as estado, EMPRD_CEP as cep, " + 
                            "EMPRD_ENTIDADE as entidade, EMPRD_REGIAO as regiao, EMPRD_TEMPRD as tipoempreendimento, " + 
                            "EMPRD_PADRAO as empreendimentopadrao, EMPRD_MUNICIPIO as codigomunicipio, " +
                            "EMPRD_DTLIDOCRM as datahoraultimaatualizacao " +
                            "FROM CADDVS_EMPREEND " +
                            $"WHERE EMPRD_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<EmpreendimentosIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<EmpreendimentosIntegracao> dadosorigem)
        {
            foreach (var item in dadosorigem)
            {
                try
                {
                    registroatual += 1;
                    if (JaEstaCadastrado(item))
                    {
                        Alterar(item);
                    }
                    else
                    {
                        Incluir(item);
                    }
                }
                catch (Exception ex)
                {
                    var mensagemErro = $"SALVANDO REGISTROS {TABELA}\n\n" +
                         $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                        $"Detalhes: {ex.Message}\n\n" +
                         (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                        Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(item);

                    IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                    _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message} ").Wait();
                }
            }
        }

        private int ProximoCodigo()
        {
            var connection = _connectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_Empreendimento)+1,1) as codigo FROM [dbo].[CAD_EMPREENDIMENTO]";
            var proximo = connection.Query<int>(sql).ToList();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: gerado novo código empreendimento: {proximo[0]}").Wait();
            return proximo[0];
        }

        private void Incluir(EmpreendimentosIntegracao obj)
        {
            try
            {
                var codigoempreendimento = ProximoCodigo();
                obj.codigopadrao = 1;                
                var connection = _connectionMSSQL;
                var sql = "" +
                        $"INSERT INTO [dbo].[CAD_EMPREENDIMENTO] " +
                        $"           ([CD_BancoDados] " +
                        $"           ,[CD_Mandante] " +
                        $"           ,[CD_Empreendimento] " +
                        $"           ,[CD_EmpreeSP7] " +
                        $"           ,[NM_Empree] " +
                        $"           ,[NM_EmpreeAbrev] " +
                        $"           ,[DS_Endereco] " +
                        $"           ,[NM_Bairro] " +
                        $"           ,[NM_Cidade] " +
                        $"           ,[NM_UF] " +
                        $"           ,[NR_CEP] " +
                        $"           ,[DT_Controle] " +
                        $"           ,[HR_Controle] " +
                        $"           ,[CD_EntidadeSP7] " +
                        $"           ,[CD_RegiaoSP7] " +
                        $"           ,[CD_TipoEmpreeSP7] " +
                        $"           ,[CD_MUNICIPIO] " +
                        $"           ,[CD_Padrao] " +
                        $"           ,[Boleto] " +
                        $"           ,[FichaFinanceira] " +
                        $"           ,[InformeRendimento] " +
                        $"           ,[Chamado] " +
                        $"           ,[idempresa]) " +
                        $"     VALUES " +
                        $"           (1 " +
                        $"           ,1 " +
                        $"           ,@codigoempreendimento " +
                        $"           ,@empreendimentosp7 " +
                        $"           ,@empreendimento " +
                        $"           ,@abreviatura " +
                        $"           ,@endereco " +
                        $"           ,@bairro " +
                        $"           ,@cidade " +
                        $"           ,@estado " +
                        $"           ,@cep " +
                        $"           ,@dataatualizacao " +
                        $"           ,@horaatualizacao " +
                        $"           ,@entidade " +
                        $"           ,@regiao " +
                        $"           ,@tipoempreendimento " +
                        $"           ,@codigomunicipio " +
                        $"           ,1 " +
                        $"           ,1 " +
                        $"           ,1" +
                        $"           ,1 " +
                        $"           ,1 " +
                        $"           ,1) ";

                var parameters = new
                {
                    codigoempreendimento = codigoempreendimento,
                    empreendimentosp7 = obj.empreendimentosp7,
                    empreendimento = obj.empreendimento,
                    abreviatura = obj.abreviatura,
                    endereco = obj.endereco,
                    bairro = obj.bairro,
                    cidade = obj.cidade,
                    estado = obj.estado,
                    cep = obj.cep,
                    dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    entidade = obj.entidade,
                    regiao = obj.regiao,
                    tipoempreendimento = obj.tipoempreendimento,
                    codigomunicipio = obj.codigomunicipio
                };
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro. empreendimento {obj.empreendimento} - {obj.empreendimentosp7}").Wait();
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
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} empreendimento {obj.empreendimento} - {obj.empreendimentosp7}").Wait();
            }
        }

        private void Alterar(EmpreendimentosIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
                var sql = "" +
                        $"UPDATE [dbo].[CAD_EMPREENDIMENTO] " +
                        $"   SET [CD_EmpreeSP7] = @empreendimentosp7 " +
                        $"      ,[NM_Empree] = @empreendimento " +
                        $"      ,[NM_EmpreeAbrev] = @abreviatura " +
                        $"      ,[DS_Endereco] = @endereco " +
                        $"      ,[NM_Bairro] = @bairro " +
                        $"      ,[NM_Cidade] = @cidade " +
                        $"      ,[NM_UF] = @estado " +
                        $"      ,[NR_CEP] = @cep " +
                        $"      ,[DT_Controle] = @dataatualizacao " +
                        $"      ,[HR_Controle] = @horaatualizacao " +
                        $"      ,[CD_EntidadeSP7] = @entidade " +
                        $"      ,[CD_RegiaoSP7] = @regiao " +
                        $"      ,[CD_TipoEmpreeSP7] = @tipoempreendimento " +
                        $"      ,[CD_MUNICIPIO] = @codigomunicipio " +
                        $"      ,[CD_Padrao] = 1 " +
                        $" WHERE [CD_EmpreeSP7] = @empreendimentosp7 ";

                var parameters = new
                {
                    empreendimentosp7 = obj.empreendimentosp7,
                    empreendimento = obj.empreendimento,
                    abreviatura = obj.abreviatura,
                    endereco = obj.endereco,
                    bairro = obj.bairro,
                    cidade = obj.cidade,
                    estado = obj.estado,
                    cep = obj.cep,
                    dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    entidade = obj.entidade,
                    regiao = obj.regiao,
                    tipoempreendimento = obj.tipoempreendimento,
                    codigomunicipio = obj.codigomunicipio
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterando registro. empreendimento {obj.empreendimento} - {obj.empreendimentosp7}").Wait();
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
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} empreendimento {obj.empreendimento} - {obj.empreendimentosp7}").Wait();
            }                        
        }

        private Boolean JaEstaCadastrado(EmpreendimentosIntegracao obj)
        {
            var connection = _connectionMSSQL;
            var sql = $"SELECT 1 FROM CAD_EMPREENDIMENTO WHERE CD_EmpreeSP7={obj.empreendimentosp7} ";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
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

    }
}
