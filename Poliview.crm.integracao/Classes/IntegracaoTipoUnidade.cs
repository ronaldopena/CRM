using Dapper;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;

namespace poliview.crm.integracao
{
    public class IntegracaoTipoUnidade : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "TIPO UNIDADE";
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private Microsoft.Data.SqlClient.SqlConnection _connectionMSSQL;
        private int totalregistros = 0;
        private int registroatual = 0;
        private string? _cliente;
        private static string _tituloMensagem = "Integração de TIPO DE UNIDADE";
        private IConfiguration _configuration;

        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoTipoUnidade(DateTime? DataHoraUltimaIntegracao,
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

                InicioIntegracao();

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: inicio de integração {_DataHoraUlimaIntegracao.ToString("dd/MM/yyyy HH:mm:ss")}").Wait();
                var registrosOrigem = this.ListarOrigem();
                totalregistros = registrosOrigem.Count;
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: foram encontrados {registrosOrigem.Count} registros para integração").Wait();
                if (registrosOrigem != null)
                {
                    salvarDadosDestino(registrosOrigem);
                }
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: fim de integração").Wait();

                ExcluirRegistros();

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

        private void InicioIntegracao()
        {
            var connection = _connectionMSSQL;
            var sql = $"UPDATE [dbo].[CAD_TIPO_UNIDADE] SET integrado=0 ";
            connection.ExecuteAsync(sql).Wait();
        }

        private void ExcluirRegistros()
        {
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluindo registro.").Wait();
            var connection = _connectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_TIPO_UNIDADE] WHERE integrado=0";
            connection.ExecuteAsync(sql).Wait();            
        }

        private List<TipoUnidadeIntegracao> ListarOrigem()
        {
            var connection = _connectionFB;
            var sql = "SELECT TIPO_CDG as id, TIPO_DESC as descricao FROM EMP_TIPO";
            return connection.Query<TipoUnidadeIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<TipoUnidadeIntegracao> dadosorigem)
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
                    $"Detalhes: {ex.Message}\n\n" +
                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                    Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(item);
                    _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message} ").Wait();
                }
            }
        }

        private void Incluir(TipoUnidadeIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
                var sql = $"INSERT INTO [dbo].[CAD_TIPO_UNIDADE] ([id] ,[descricao], [integrado]) VALUES (@id, @descricao, 1) ";
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro. id {obj.id} descricao {obj.descricao}").Wait();
                var parameters = new
                {
                    id = obj.id,
                    descricao = obj.descricao
                };

                connection.ExecuteAsync(sql, parameters).Wait();
            }
            catch (Exception ex)
            {
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} id {obj.id} descricao {obj.descricao}").Wait();
            }
        }

        private void Alterar(TipoUnidadeIntegracao obj)
        {
            try
            {
                var connection = _connectionMSSQL;
                var sql = "" +
                        $"UPDATE [dbo].[CAD_TIPO_UNIDADE] " +
                        $"   SET [descricao] = @descricao, [integrado]=1 " +
                        $" WHERE id=@id ";
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterando registro. id {obj.id} descricao {obj.descricao}").Wait();
                var parameters = new
                {
                    id = obj.id,
                    descricao = obj.descricao
                };
                connection.ExecuteAsync(sql, parameters).Wait();
            }
            catch (Exception ex)
            {
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} id {obj.id} descricao {obj.descricao}").Wait();
            }
        }

        private Boolean JaEstaCadastrado(TipoUnidadeIntegracao obj)
        {
            var connection = _connectionMSSQL;
            var sql = $"select 1 from CAD_TIPO_UNIDADE where id={obj.id}";
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
