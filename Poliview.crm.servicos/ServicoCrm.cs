using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Poliview.crm.servicos
{
    public partial class ServicoCrm : ServiceBase
    {
        private Timer timer;
        private string _nomeServico = "Poliview CRM Serviço - ";
        private string _connectionString = "";
        private string _nomeCliente = "";
        private bool _executando = false;

        public class ServicosCRM
        {
            public string NomeServico { get; set; }
            public string CaminhoServico { get; set; }
            public string ExecutavelServico { get; set; }
            public DateTime DataUltimaExecucao { get; set; }
            public DateTime DataUltimoProcessamento { get; set; }
        }

        public ServicoCrm()
        {
            try
            {
                this._connectionString = ConfigurationManager.AppSettings["conexao"];
                this._nomeCliente = ConfigurationManager.AppSettings["cliente"];

                if (string.IsNullOrEmpty(this._connectionString))
                {
                    throw new Exception("String de conexão não encontrada no App.config");
                }

                if (string.IsNullOrEmpty(this._nomeCliente))
                {
                    throw new Exception("Nome do cliente não encontrado no App.config");
                }

                this._nomeServico = "Poliview CRM Serviço - " + this._nomeCliente;
                InitializeComponent(_nomeCliente);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Poliview CRM Serviço", $"Erro na inicialização: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry(_nomeServico, $"Iniciado serviço {_nomeServico}", EventLogEntryType.Information);

                // Testa a conexão na inicialização
                TestarConexao();

                timer = new Timer(new TimerCallback(Execute), null, 0, 60000);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_nomeServico, $"Erro ao iniciar serviço: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry(_nomeServico, $"Parado serviço {_nomeServico}", EventLogEntryType.Information);
            timer?.Dispose();
        }

        private void TestarConexao()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    EventLog.WriteEntry(_nomeServico, "Conexão com banco de dados testada com sucesso", EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_nomeServico, $"Erro na conexão com banco de dados: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        private void Execute(object state)
        {


            try
            {
                var result = this.ListarServicos();

                if (result != null && result.Any())
                {
                    foreach (ServicosCRM item in result)
                    {
                        IniciarProcesso(item.NomeServico, item.CaminhoServico, item.ExecutavelServico);
                    }
                }
                else
                {
                    EventLog.WriteEntry(_nomeServico, "Nenhum serviço ativo encontrado", EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_nomeServico, $"Erro na execução: {ex.Message}", EventLogEntryType.Error);
            }
            finally
            {
                _executando = false;
            }
        }

        private void IniciarProcesso(string nomeServico, string caminho, string executavel)
        {
            if (string.IsNullOrEmpty(caminho) || string.IsNullOrEmpty(executavel))
            {
                EventLog.WriteEntry(_nomeServico, $"Caminho ou executável inválido para o serviço {nomeServico}", EventLogEntryType.Error);
                return;
            }

            var caminhoCompleto = Path.GetFullPath(Path.Combine(caminho, executavel));

            if (ProcessoJaEmExecucao(caminhoCompleto))
            {
                EventLog.WriteEntry(_nomeServico, $"O processo já está em execução no servidor: {caminhoCompleto}", EventLogEntryType.Information);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = caminhoCompleto,
                WorkingDirectory = caminho
            };

            try
            {
                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        EventLog.WriteEntry(_nomeServico, $"Falha ao iniciar o processo {nomeServico}.", EventLogEntryType.Error);
                        return;
                    }

                    int pid = process.Id;
                    EventLog.WriteEntry(_nomeServico, $"Iniciado processo {pid} {nomeServico} - {startInfo.FileName}", EventLogEntryType.Information);

                    this.GravarDataExecucao(nomeServico);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_nomeServico, $"Erro ao gerar processo para {nomeServico}: {ex.Message}", EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Verifica se já existe um processo em execução com o mesmo caminho do executável.
        /// </summary>
        private static bool ProcessoJaEmExecucao(string caminhoExecutavel)
        {
            if (string.IsNullOrWhiteSpace(caminhoExecutavel))
                return false;

            var caminhoNormalizado = Path.GetFullPath(caminhoExecutavel).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            try
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (process.MainModule != null && !string.IsNullOrEmpty(process.MainModule.FileName))
                        {
                            var pathProcesso = Path.GetFullPath(process.MainModule.FileName).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            if (string.Equals(pathProcesso, caminhoNormalizado, StringComparison.OrdinalIgnoreCase))
                            {
                                process.Dispose();
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // Acesso negado ou processo encerrado; ignorar e seguir
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception)
            {
                // Em caso de falha ao listar processos, considera que não está em execução e deixa o IniciarProcesso tentar iniciar
                return false;
            }

            return false;
        }

        private void GravarDataExecucao(string nomeServico)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = "UPDATE OPE_SERVICOS SET DataUltimaExecucao = GETDATE() WHERE NomeServico = @NomeServico";
                    connection.Execute(query, new { NomeServico = nomeServico });
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_nomeServico, $"Erro ao gravar data de execução para {nomeServico}: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private List<ServicosCRM> ListarServicos()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = "SELECT NomeServico, CaminhoServico, ExecutavelServico, DataUltimaExecucao, DataUltimoProcessamento FROM OPE_SERVICOS WHERE Ativo='S'";

                    var result = connection.Query<ServicosCRM>(query).ToList();

                    EventLog.WriteEntry(_nomeServico, $"Número de serviços ativos encontrados: {result.Count}", EventLogEntryType.Information);

                    return result;
                }
            }
            catch (SqlException sqlEx)
            {
                EventLog.WriteEntry(_nomeServico, $"Erro SQL ao listar serviços: {sqlEx.Message}\nNúmero do erro: {sqlEx.Number}\nSeveridade: {sqlEx.Class}", EventLogEntryType.Error);
                return new List<ServicosCRM>();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(_nomeServico, $"Erro geral ao listar serviços: {ex.Message}\nStackTrace: {ex.StackTrace}", EventLogEntryType.Error);
                return new List<ServicosCRM>();
            }
        }
    }
}

