using Dapper;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;
using System.Linq.Expressions;

namespace poliview.crm.integracao
{
    public class IntegracaoFornecedores : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "FORNECEDORES";
        private int totalregistros = 0;
        private int registroatual = 0;
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private Microsoft.Data.SqlClient.SqlConnection _connectionMSSQL;

        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoFornecedores(DateTime? DataHoraUltimaIntegracao,
                                  DateTime DataHoraAtual,
                                  int CodigoTabela,
                                  string connectionStringMssql,
                                  string connectionStringFb,
                                  LogService logService)
        {
            _logService = logService;
            _DataHoraUlimaIntegracao = (DateTime)(DataHoraDaUltimaIntegracao == null ? DataHoraDaUltimaIntegracao() : DataHoraUltimaIntegracao);
            _connectionStringFb = connectionStringFb;
            _connectionStringMssql = connectionStringMssql;
            _CodigoTabela = CodigoTabela;
            _DataHoraAtual = DataHoraAtual;
            _connectionFB = new FirebirdConnectionFactory(_connectionStringFb).CreateConnection();
            _connectionMSSQL = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
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
                return true;
            }
            catch (Exception ex)
            {
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

                        ExcluirRegistro(chave[0]);

                    }
                }
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: fim de integração").Wait();
                return true;
            }
            catch (Exception ex)
            {
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message}").Wait();
                return false;
            }   
        }

        private void ExcluirRegistro(string fornecedor)
        {
            var connection = _connectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_FORNECEDOR] WHERE AND CD_FornecedorSP7='{fornecedor}' ";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro. fornecedor: {fornecedor} ").Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _connectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='FORNECEDORES'";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private List<FornecedoresIntegracao> ListarOrigem()
        {
            var connection = _connectionFB;
            var sql = "SELECT " +
                    "FORN_CNPJ as codigofornecedorsp7, FORN_RAZAO as nome, FORN_CPFCNPJ as cpfcnpj, FORN_EMAIL1 as email, FORN_ENDERECO as endereco, " +
                    "FORN_BAIRRO as bairro, FORN_CIDADE as cidade, FORN_UF as estado, FORN_CEP as cep, FORN_DDD as ddd, FORN_TELEFONE1 as telefone, " +
                    "FORN_CELULAR1 as celular, FORN_CONTATO1 as contato, BLOCO_DTLIDOCRM as datahoraultimaatualizacao " +
                    "FROM CADCPG_FORNECEDOR " +
                    $"WHERE FORN_TPFORNECEDOR=1 AND FORN_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            return connection.Query<FornecedoresIntegracao>(sql).ToList();
        }

        private void salvarDadosDestino(List<FornecedoresIntegracao> dadosorigem)
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
                    _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message} ").Wait();
                }
            }
        }

        private void Incluir(FornecedoresIntegracao obj)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = "" +
                    $"INSERT INTO [dbo].[CAD_FORNECEDOR] " +
                    $"           ([CD_BancoDados] " +
                    $"           ,[CD_Mandante] " +
                    $"           ,[CD_FornecedorSP7] " +
                    $"           ,[NM_Fornecedor] " +
                    $"           ,[NR_CPFCNPJ] " +
                    $"           ,[DS_Email] " +
                    $"           ,[DS_Endereco] " +
                    $"           ,[NM_Bairro] " +
                    $"           ,[NM_Cidade] " +
                    $"           ,[NM_UF] " +
                    $"           ,[NR_CEP] " +
                    $"           ,[NR_DDD] " +
                    $"           ,[NR_Telefone] " +
                    $"           ,[NR_Celular] " +
                    $"           ,[DT_Controle] " +
                    $"           ,[HR_Controle]) " +
                    $"     VALUES " +
                    $"           (1 " +
                    $"           ,1 " +
                    $"           ,'{obj.codigofornecedorsp7}' " +
                    $"           ,'{obj.nome}' " +
                    $"           ,'{obj.cpfcnpj}' " +
                    $"           ,'{obj.email}' " +
                    $"           ,'{obj.endereco}' " +
                    $"           ,'{obj.bairro}' " +
                    $"           ,'{obj.cidade}' " +
                    $"           ,'{obj.estado}' " +
                    $"           ,'{obj.cep}' " +
                    $"           ,'{obj.ddd}' " +
                    $"           ,'{obj.telefone}' " +
                    $"           ,'{obj.celular}' " +
                    $"           ,'{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
                    $"           ,'{obj.datahoraultimaatualizacao.ToString("HH:mm")}' )";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluido registro. ").Wait();
        }

        private void Alterar(FornecedoresIntegracao obj)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = "" +
                    $"UPDATE [dbo].[CAD_FORNECEDOR] " +
                    $"   SET [CD_FornecedorSP7] = '{obj.codigofornecedorsp7}' " +
                    $"      ,[NM_Fornecedor] = '{obj.nome}' " +
                    $"      ,[NR_CPFCNPJ] = '{obj.cpfcnpj}' " +
                    $"      ,[DS_Email] = '{obj.email}' " +
                    $"      ,[DS_Endereco] = '{obj.endereco}' " +
                    $"      ,[NM_Bairro] = '{obj.bairro}' " +
                    $"      ,[NM_Cidade] = '{obj.cidade}' " +
                    $"      ,[NM_UF] = '{obj.estado}' " +
                    $"      ,[NR_CEP] = '{obj.cep}' " +
                    $"      ,[NR_DDD] = '{obj.ddd}' " +
                    $"      ,[NR_Telefone] = '{obj.telefone}' " +
                    $"      ,[NR_Celular] = '{obj.celular}' " +
                    $"      ,[DT_Controle] = '{obj.datahoraultimaatualizacao.ToString("yyyyMMdd")}' " +
                    $"      ,[HR_Controle] = '{obj.datahoraultimaatualizacao.ToString("HH:mm")}' " +
                    $" WHERE CD_FornecedorSP7='{obj.codigofornecedorsp7}' ";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterado registro. ").Wait();
        }

        private Boolean JaEstaCadastrado(FornecedoresIntegracao obj)
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = $"select 1 from CADCPG_FORNECEDOR where FORN_CNPJ={obj.codigofornecedorsp7} ";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }

        private DateTime DataHoraDaUltimaIntegracao()
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = $"select DataUltimaIntegracao from CAD_INTEGRACAO where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            return connection.QueryFirstOrDefault<DateTime>(sql);
        }
        private void AlterarDataHoraDaUltimaIntegracao()
        {
            var connection = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            var sql = $"UPDATE CAD_INTEGRACAO SET integrar=0, DataUltimaIntegracao='{_DataHoraAtual.ToString("yyyy-MM-dd HH:mm:ss")}' where CD_Tabela={_CodigoTabela} AND CD_BANCODADOS=1 AND CD_MANDANTE=1 ";
            connection.ExecuteAsync(sql).Wait();
        }


    }
}
