using Dapper;
using Microsoft.Data.SqlClient;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;

namespace poliview.crm.integracao
{
    public class IntegracaoBlocos : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "BLOCOS";
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private SqlConnection _connectionMSSQL;
        private int totalregistros = 0;
        private int registroatual = 0;
        private readonly string? _cliente;
        private static string _tituloMensagem = "Integração de BLOCOS";
        private IConfiguration _configuration;

        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoBlocos(DateTime? DataHoraUltimaIntegracao,
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
                        }
                        catch (Exception ex)
                        {
                            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: Exclusão de registros {chave}").Wait();
                        }
                    }
                }
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: Exclusão de registros").Wait();
                return true;
            }
            catch (Exception ex)
            {
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message}").Wait();
                return false;
            }

        }

        private void ExcluirRegistro(string empreendimento, string bloco)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_BLOCO] WHERE CD_EmpreeSP7={empreendimento} AND CD_BlocoSP7={bloco}";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro. empreendimento: {empreendimento} bloco: {bloco}").Wait();
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO WHERE TABELA='BLOCOS' and CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro processado. tabela: BLOCOS chave: {chave} ").Wait();
        }

        private void ExcluirRegistroOrigemNulo()
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO WHERE TABELA='BLOCO' and CHAVE is null";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro nulo ").Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            //var connection = new FirebirdConnectionFactory(_connectionStringFb).CreateConnection();
            var connection = _connectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='BLOCOS'";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private List<BlocosIntegracao> ListarOrigem()
        {
            var connection = _connectionFB;
            var sql = "SELECT " +
                    "BLOCO_EMPRD as codigoempreendimentosp7, BLOCO_CDG as codigoblocosp7, BLOCO_DESC as nome, " +
                    "BLOCO_ABREV as abreviatura, BLOCO_END as endereco, BLOCO_BAIRRO as bairro, BLOCO_CID as cidade, BLOCO_UF as estado, BLOCO_CEP as cep, " +
                    "BLOCO_DTTERMINO as datahabitese, BLOCO_DTLIDOCRM as datahoraultimaatualizacao " +
                    "FROM CADDVS_BLOCO " +
                    $"WHERE BLOCO_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";            
            return connection.Query<BlocosIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<BlocosIntegracao> dadosorigem)
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
                        var codigo = ProximoCodigo();
                        item.idbloco = codigo;
                        IncluirOpeBloco(item);
                        IncluirCadBloco(item);
                    }
                }
                catch (Exception ex)
                {
                    _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message}").Wait();
                }
            }
        }

        private int ProximoCodigo()
        {            
            var connection = _connectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_Bloco)+1,1) as codigo FROM [dbo].[CAD_BLOCO]";
            var proximo = connection.Query<int>(sql).ToList();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: gerado novo código bloco: {proximo[0]}").Wait();
            return proximo[0];
        }

        private int CodigoEmpreendimento(int codigoempreendimentosp7)
        {
            var connection = _connectionMSSQL;
            var sql = $"SELECT CD_Empreendimento FROM [dbo].[CAD_EMPREENDIMENTO] where CD_EmpreeSP7={codigoempreendimentosp7}";
            var codigo = connection.Query<int>(sql).ToList();            
            return codigo[0];
        }

        private void IncluirOpeBloco(BlocosIntegracao obj)
        {
            var codigoempreendimento = CodigoEmpreendimento(obj.codigoempreendimentosp7);
            try
            {
                var connection = _connectionMSSQL;
                var sql = "" +
                        $"INSERT INTO [dbo].[OPE_BLOCO] " +
                        $"           ([CD_BancoDados] " +
                        $"           ,[CD_Mandante] " +
                        $"           ,[CD_Empreendimento] " +
                        $"           ,[CD_Bloco] " +
                        $"           ,[DT_Controle]) " +
                        $"     VALUES " +
                        $"           (1 " +
                        $"           ,1 " +
                        $"           ,@codigoemprd " +
                        $"           ,@codigobloco " +
                        $"           ,getDate() )";

                var parameters = new
                {
                    codigo = obj.idbloco,
                    codigoemprd = codigoempreendimento,
                    codigobloco = obj.idbloco
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro OPE_BLOCO. bloco {obj.idbloco} - {obj.nome}").Wait();
                connection.ExecuteAsync(sql, parameters).Wait();
            }
            catch (Exception ex)
            {
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: OPE_BLOCO ERRO {ex.Message} bloco {obj.codigoblocosp7} - {obj.nome}").Wait();
            }
        }


        private void IncluirCadBloco(BlocosIntegracao obj)
        {
            try
            {                
                var connection = _connectionMSSQL;
                var sql = "" +
                        $"INSERT INTO [dbo].[CAD_BLOCO] " +
                        $"           ([CD_BancoDados] " +
                        $"           ,[CD_Mandante] " +
                        $"           ,[CD_Bloco] " +
                        $"           ,[CD_EmpreeSP7] " +
                        $"           ,[CD_BlocoSP7] " +
                        $"           ,[NM_Bloco] " +
                        $"           ,[NM_BlocoAbrev] " +
                        $"           ,[DS_Endereco] " +
                        $"           ,[NM_Bairro] " +
                        $"           ,[NM_Cidade] " +
                        $"           ,[NM_UF] " +
                        $"           ,[NR_CEP] " +
                        $"           ,[DT_Habitese] " +
                        $"           ,[DT_Controle] " +
                        $"           ,[HR_Controle]) " +
                        $"     VALUES " +
                        $"           (1 " +
                        $"           ,1 " +
                        $"           ,@codigo " +
                        $"           ,@codigoempreendimentosp7 " +
                        $"           ,@codigoblocosp7 " +
                        $"           ,@nome " +
                        $"           ,@abreviatura " +
                        $"           ,@endereco " +
                        $"           ,@bairro " +
                        $"           ,@cidade " +
                        $"           ,@estado " +
                        $"           ,@cep " +
                        $"           ,@datahabitese " +
                        $"           ,@dataatualizacao " +
                        $"           ,@horaatualizacao )";

                var parameters = new
                {
                    codigo=obj.idbloco,
                    codigoempreendimentosp7 = obj.codigoempreendimentosp7,
                    codigoblocosp7 = obj.codigoblocosp7,
                    nome = obj.nome,
                    abreviatura = obj.abreviatura,
                    endereco = obj.endereco,
                    bairro = obj.bairro,
                    cidade = obj.cidade,
                    estado = obj.estado,
                    cep = obj.cep,
                    dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    datahabitese = obj.datahabitese,
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro. bloco {obj.codigoblocosp7} - {obj.nome}").Wait();
                connection.ExecuteAsync(sql, parameters).Wait();
            }
            catch (Exception ex)
            {
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} bloco {obj.codigoblocosp7} - {obj.nome}").Wait();
            }
        }

        private void Alterar(BlocosIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
                var sql = "" +
                        $"UPDATE [dbo].[CAD_BLOCO] " +
                        $"   SET [CD_EmpreeSP7] = @codigoempreendimentosp7 " +
                        $"      ,[CD_BlocoSP7] = @codigoblocosp7 " +
                        $"      ,[NM_Bloco] = @nome " +
                        $"      ,[NM_BlocoAbrev] = @abreviatura " +
                        $"      ,[DS_Endereco] = @endereco " +
                        $"      ,[NM_Bairro] = @bairro " +
                        $"      ,[NM_Cidade] = @cidade " +
                        $"      ,[NM_UF] = @estado " +
                        $"      ,[NR_CEP] = @cep " +
                        $"      ,[DT_Controle] = @dataatualizacao " +
                        $"      ,[HR_Controle] = @horaatualizacao " +
                        $"      ,[DT_Habitese] = @datahabitese " +
                        $" WHERE [CD_EmpreeSP7]=@codigoempreendimentosp7 and  [CD_BlocoSP7]=@codigoblocosp7";
                
                if (obj.datahabitese.Year < 1900)
                {
                    obj.datahabitese = new DateTime(1900, 1, 1);
                }

                var parameters = new
                {
                    codigoempreendimentosp7 = obj.codigoempreendimentosp7,
                    codigoblocosp7 = obj.codigoblocosp7,
                    nome = obj.nome,
                    abreviatura = obj.abreviatura,
                    endereco = obj.endereco,
                    bairro = obj.bairro,
                    cidade = obj.cidade,
                    estado = obj.estado,
                    cep = obj.cep,
                    dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    datahabitese = obj.datahabitese,
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterando registro. bloco {obj.codigoblocosp7} - {obj.nome}").Wait();
                connection.ExecuteAsync(sql, parameters).Wait();
            }
            catch (Exception ex)
            {
                Poliview.crm.infra.Util.ExibirPropriedades(obj);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} bloco {obj.codigoblocosp7} - {obj.nome}").Wait();
            }
        }

        private Boolean JaEstaCadastrado(BlocosIntegracao obj)
        {
            // var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var connection = _connectionMSSQL;
            var sql = $"select 1 from CAD_BLOCO where CD_BLOCOSP7={obj.codigoblocosp7} AND CD_EmpreeSP7={obj.codigoempreendimentosp7}";
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
