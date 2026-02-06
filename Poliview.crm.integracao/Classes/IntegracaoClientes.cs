using Dapper;
using Microsoft.Data.SqlClient;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using Poliview.crm.infra;
using System.Diagnostics;

namespace poliview.crm.integracao
{
    public class IntegracaoClientes : IIntegracao
    {
        private int _CodigoTabela;
        private DateTime _DataHoraUlimaIntegracao;
        private DateTime _DataHoraAtual;
        private string _connectionStringMssql;
        private string _connectionStringFb;
        private readonly LogService _logService;
        private string TABELA = "CLIENTES";
        private FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;
        private SqlConnection _connectionMSSQL;
        private int totalregistros = 0;
        private int registroatual = 0;
        private string? _cliente;
        private static string _tituloMensagem = "Integração de BLOCOS";
        private IConfiguration _configuration;

        public class validacao
        {
            public string? codigoclientesp7 { get; set; }
        }

        public IntegracaoClientes(DateTime? DataHoraUltimaIntegracao,
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

                var clientesFaltandoNoContrato = ContratosSemClientes();
                var registrosOrigem = this.ListarOrigem(clientesFaltandoNoContrato);

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
                            ExcluirRegistro(chave[0]);
                            ExcluirRegistroProcessado(item.chave);

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
                var mensagemErro = $"EXCLUINDO REGISTROS {TABELA}\n\n" +
                     $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                $"Detalhes: {ex.Message}\n\n" +
                (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message}").Wait();
                return false;
            }
        }

        private void ExcluirRegistro(string cliente)
        {
            var connection = _connectionMSSQL;
            var sql = $"DELETE FROM [dbo].[CAD_CLIENTE] WHERE CD_ClienteSP7='{cliente}'";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro. cliente: {cliente}").Wait();
        }

        private int ProximoCodigoCliente()
        {
            var connection = _connectionMSSQL;
            var sql = $"SELECT COALESCE(MAX(CD_CLIENTE)+1,1) as codigo FROM [dbo].[CAD_CLIENTE]";
            var proximo = connection.Query<int>(sql).ToList();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: gerado novo código cliente: {proximo[0]}").Wait();
            return proximo[0];
        }

        private void ExcluirRegistroProcessado(string chave)
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO WHERE TABELA='CLIENTES' and CHAVE='{chave}'";
            connection.ExecuteAsync(sql).Wait();
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: excluido registro processado. tabela: CLIENTES chave: {chave} ").Wait();
        }

        private List<ExclusaoIntegracao> ListarOrigemExclusao()
        {
            var connection = _connectionFB;
            var sql = $"SELECT * FROM CRM_EXCLUSAO where tabela='CLIENTES'";
            return connection.Query<ExclusaoIntegracao>(sql).ToList();
        }

        private string ContratosSemClientes()
        {
            var connection = _connectionMSSQL;
            var sql = @$"
                            SELECT STUFF(
                                        (
                                            SELECT ',' + QUOTENAME(con.CD_ClienteSP7, '''')
                                            FROM (
                                                SELECT DISTINCT TOP 20 con.CD_ClienteSP7 
                                                FROM CAD_CONTRATO con
                                                LEFT JOIN CAD_CLIENTE cli ON cli.CD_ClienteSP7 = con.CD_ClienteSP7
                                                WHERE cli.CD_ClienteSP7 IS NULL
                                            ) con
                                            FOR XML PATH(''), TYPE
                                        ).value('.', 'NVARCHAR(MAX)'), 
                                        1, 1, ''
                                    ) AS ListaClientes
                        ";
            var registro = connection.QueryFirst<string>(sql);
            return registro;
        }


        private List<ClientesIntegracao> ListarOrigem(string clientes)
        {
            var connection = _connectionFB;
            var sql = "SELECT " +
                           "FORN_CNPJ as codigoclientesp7, FORN_RAZAO as nome, FORN_CPFCNPJ as cpfcnpj, FORN_EMAIL as email, " +
                           "FORN_TPLOGRADOURORES||' '||FORN_LOGRADOURORES||', '||FORN_NUMERORES AS endereco, " +
                           "FORN_BAIRRORES AS bairro, FORN_CIDRES AS cidade, FORN_UF as estado, FORN_CEPRES AS cep, " +
                           "FORN_DDDCEL1 as ddd, FORN_TELEFONE1 as telefone, FORN_CELULAR1 as celular, FORN_CONTATO1 as contato, FORN_DTNASCIMENTO as datanascimento, " +
                           "FORN_DTLIDOCRM as datahoraultimaatualizacao " +
                           "FROM CADCPG_FORNECEDOR " +
                            // $"WHERE FORN_TPCLIENTE=1 AND FORN_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
                            //$"WHERE FORN_TPCLIENTE=1 AND FORN_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}' and FORN_CNPJ IN ('00000000010400')";
                            $"WHERE FORN_TPCLIENTE=1 AND FORN_DTLIDOCRM>='{_DataHoraUlimaIntegracao.ToString("yyyy-MM-dd HH:mm:ss")}'";
            if (!string.IsNullOrEmpty(clientes))
            {
                sql += " OR (FORN_CNPJ IN (" + clientes + "))";
            }
                            
            return connection.Query<ClientesIntegracao>(sql).ToList();
        }


        private void salvarDadosDestino(List<ClientesIntegracao> dadosorigem)
        {
            var cli = new ClientesIntegracao();
            try
            {
                foreach (var item in dadosorigem)
                {
                    item.email = Util.LimparListaEmails(item.email);

                    if (item.datanascimento.Year < 1900 && item.datanascimento.Year > 1)
                    {
                        // Poliview.crm.infra.Util.ExibirPropriedades(cli);
                        item.datanascimento = new DateTime(1900, item.datanascimento.Month, item.datanascimento.Day);
                    }
                    cli = item;
                    registroatual += 1;
                    if (JaEstaCadastrado(item))
                    {
                        Alterar(item);
                    }
                    else
                    {
                        Incluir(item);
                    }

                    AlterarEmailUsuario(item);
                }
            }
            catch (Exception ex)
            {
                var mensagemErro = $"SALVANDO REGISTROS {TABELA}\n\n" +
                     $"Cliente: {_cliente ?? "não identificado"}\n\n" +
                    $"Detalhes: {ex.Message}\n\n" +
                    (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") + "\n\n" +
                    Poliview.crm.infra.Util.ExibirPropriedadesNotificacao(cli);

                IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                Poliview.crm.infra.Util.ExibirPropriedades(cli);
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: {ex.Message} ").Wait();
            }
        }

        private void Incluir(ClientesIntegracao obj)
        {
            var sql = "";
            try
            {
                if (obj.endereco != null)
                {
                    if (obj.endereco == " , ") obj.endereco = "";
                }

                var dtnascimento = obj.datanascimento.ToString("yyyy-MM-dd");

                try
                {
                    DateTime data = DateTime.ParseExact(dtnascimento, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    int ano = data.Year;
                    if (ano < 1000) dtnascimento = "0001-01-01";
                }
                catch (Exception)
                {
                    dtnascimento = "0001-01-01";
                }

                dtnascimento = dtnascimento == "0001-01-01" ? null : dtnascimento;
                var codigocliente = ProximoCodigoCliente();
                var connection = _connectionMSSQL;
                sql = "set dateformat YMD; " +
                $"INSERT INTO [dbo].[CAD_CLIENTE] " +
                $"           ([CD_BancoDados] " +
                $"           ,[CD_Mandante] " +
                $"           ,[CD_ClienteSP7] " +
                $"           ,[CD_Cliente] " +
                $"           ,[NM_Cliente] " +
                $"           ,[NM_ClienteFantasia] " +
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
                $"           ,[NM_Contato] " +
                $"           ,[DT_Controle] " +
                $"           ,[HR_Controle] " +
                $"           ,[IN_ContratoResp] " +
                $"           ,[Data_Nascimento]) " +
                $"     VALUES " +
                $"           (1 " +
                $"           ,1 " +
                $"           ,@codigoclientesp7 " +
                $"           ,@codigocliente " +
                $"           ,@nome " +
                $"           ,@nome " +
                $"           ,@cpfcnpj " +
                $"           ,@email " +
                $"           ,@endereco " +
                $"           ,@bairro " +
                $"           ,@cidade " +
                $"           ,@estado " +
                $"           ,@cep " +
                $"           ,@ddd " +
                $"           ,@telefone  " +
                $"           ,@celular " +
                $"           ,@contato " +
                $"           ,@dataatualizacao " +
                $"           ,@horaatualizacao " +
                $"           ,0 " +
                $"           ,@datanascimento); ";

                var parameters = new
                {
                    codigoclientesp7 = obj.codigoclientesp7,
                    codigocliente = codigocliente,
                    nome = obj.nome,
                    cpfcnpj = obj.cpfcnpj,
                    email = obj.email,
                    endereco = obj.endereco,
                    bairro = obj.bairro,
                    cidade = obj.cidade,
                    estado = obj.estado,
                    cep = obj.cep,
                    ddd = obj.ddd,
                    telefone = obj.telefone,
                    celular = obj.celular,
                    contato = obj.contato,
                    dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    datanascimento = dtnascimento
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - incluindo registro. {obj.nome} - {obj.codigoclientesp7}").Wait();
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

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.debug, $"{sql}").Wait();
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} | {obj.nome} - {obj.codigoclientesp7} - {sql}").Wait();
            }
        }

        private clientesConsulta ConsultarDadosCliente(string codigoclientesp7)
        {
            var connection = _connectionMSSQL;
            var sql = $"select coalesce(DS_Email,'') as email, coalesce(NR_DDD,'') as ddd, coalesce(NR_Telefone,'') as telefone, coalesce(NR_Celular,'') as celular from CAD_CLIENTE where CD_CLIENTESP7='{codigoclientesp7}'";
            return connection.QueryFirst<clientesConsulta>(sql);
        }

        private void Alterar(ClientesIntegracao obj, Boolean gerarlog = true)
        {
            
            try
            {
                if (obj.codigoclientesp7 == "00000000000009")
                {
                    obj.cep = obj.cep;
                }
                var dadosatuaiscliente = this.ConsultarDadosCliente(obj.codigoclientesp7);

                var connection = _connectionMSSQL;

                if (obj.endereco != null)
                {
                    if (obj.endereco == " , ") obj.endereco = "";
                }

                if (obj.ddd == null) obj.ddd = "";

                if (String.IsNullOrEmpty(obj.email)) obj.email = dadosatuaiscliente.email;
                if (String.IsNullOrEmpty(obj.ddd))
                {
                    if (!String.IsNullOrEmpty(dadosatuaiscliente.ddd)) 
                        obj.ddd = dadosatuaiscliente.ddd;
                    else
                        obj.ddd = "";
                }

                obj.email = Util.LimparListaEmails(obj.email);

                if (String.IsNullOrEmpty(obj.telefone)) obj.telefone = dadosatuaiscliente.telefone;
                if (String.IsNullOrEmpty(obj.celular)) obj.celular = dadosatuaiscliente.celular;

                if (obj.ddd.Length > 2) obj.ddd = obj.ddd.Substring(0, 2);
                
                var sql = "SET DATEFORMAT YMD; ";

                sql += $"UPDATE [dbo].[CAD_CLIENTE] ";
                sql += $"   SET [NM_Cliente] = @nome ";
                sql += $"      ,[NM_ClienteFantasia] = @nome ";
                sql += $"      ,[NR_CPFCNPJ] = @cpfcnpj ";
                sql += $"      ,[DS_Email] = @email ";
                sql += $"      ,[DS_Endereco] = @endereco ";
                sql += $"      ,[NM_Bairro] = @bairro ";
                sql += $"      ,[NM_Cidade] = @cidade  ";
                sql += $"      ,[NM_UF] = @estado ";
                sql += $"      ,[NR_CEP] = @cep ";
                sql += $"      ,[NR_DDD] = @ddd ";
                sql += $"      ,[NR_Telefone] = @telefone ";
                sql += $"      ,[NR_Celular] = @celular ";
                sql += $"      ,[NM_Contato] = @contato ";
                sql += $"      ,[DT_Controle] = @dataatualizacao ";
                sql += $"      ,[HR_Controle] = @horaatualizacao ";
                if (obj.datanascimento.Year > 1900) sql += $"      ,[Data_Nascimento] = @datanascimento ";
                sql += $" WHERE [CD_ClienteSP7]=@codigoclientesp7 ";

                var parameters = new
                {
                    codigoclientesp7 = obj.codigoclientesp7,
                    nome = obj.nome,
                    cpfcnpj = obj.cpfcnpj?.Trim(),
                    email = obj.email?.Trim(),
                    endereco = obj.endereco,
                    bairro = obj.bairro,
                    cidade = obj.cidade,
                    estado = obj.estado,
                    cep = obj.cep,
                    ddd = obj.ddd,
                    telefone = obj.telefone,
                    celular = obj.celular,
                    contato = obj.contato,
                    dataatualizacao = obj.datahoraultimaatualizacao.ToString("yyyyMMdd"),
                    horaatualizacao = obj.datahoraultimaatualizacao.ToString("HH:mm"),
                    datanascimento = obj.datanascimento.ToString("yyyy-MM-dd")
                };

                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, $"{TABELA}: {registroatual} de {totalregistros} - alterando registro. {obj.nome} - {obj.codigoclientesp7}").Wait();
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
                _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"{TABELA}: ERRO {ex.Message} | {obj.nome} - {obj.codigoclientesp7}").Wait();
            }                      
        }

        private Boolean JaEstaCadastrado(ClientesIntegracao obj)
        {
            var connection = _connectionMSSQL;
            var sql = $"select 1 from CAD_CLIENTE where CD_CLIENTESP7='{obj.codigoclientesp7}'";
            var ret = connection.Query(sql);
            return (ret.Count() > 0);
        }

        private void AlterarEmailUsuario(ClientesIntegracao obj)
        {
            if (!string.IsNullOrEmpty(obj.email))
            {
                var connection = _connectionMSSQL;
                var emails = Util.ExtrairListaEmails(obj.email);
                var sql = $"update OPE_USUARIO set DS_Email=@email where NR_CPFCNPJ=@cpfcnpj";
                var parameters = new { email = emails[0], cpfcnpj = obj.cpfcnpj };
                connection.ExecuteAsync(sql, parameters).Wait();
            }
        }

        private List<validacao> ListarSiecon()
        {
            var connection = _connectionFB;
            var sql = "SELECT FORN_CNPJ as codigoclientesp7 FROM CADCPG_FORNECEDOR WHERE FORN_TPCLIENTE=1 ";
            return connection.Query<validacao>(sql).ToList();
        }

        private List<validacao> ListarCrm()
        {
            var connection = _connectionMSSQL; ;
            var sql = "SELECT CD_CLIENTESP7 as codigoclientesp7 FROM CAD_CLIENTE ";
            return connection.Query<validacao>(sql).ToList();
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

        private string retornaString(string str, int tamanho)
        {
            if (str == null) return "";

            str = str.Replace("'", "''");

            if (str.Length > tamanho) return str.Substring(0, tamanho);
            
            return str;
        }
    }
}
