using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.CodeDom.Compiler;


namespace PoliviewCRM.Gerenciador.Service
{
    public partial class Gerenciador : ServiceBase
    {
        private Timer timer;
        private string nomeCliente = "Poliview CRM";
        private string logFilePath;
        private bool isDebugMode = false;
        public Gerenciador()
        {
            nomeCliente = ConfigurationManager.AppSettings["Cliente"];

            // Verificar o nível de log configurado no App.config
            string nivelLog = ConfigurationManager.AppSettings["NivelLog"] ?? "Normal";
            isDebugMode = nivelLog.Trim().ToUpper() == "DEBUG";

            InitializeComponent(nomeCliente);

            // Definir o caminho do arquivo de log na pasta de instalação do serviço
            string pastaInstalacao = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            logFilePath = Path.Combine(pastaInstalacao, "logs");

            // Criar pasta de logs se não existir
            if (!Directory.Exists(logFilePath))
            {
                Directory.CreateDirectory(logFilePath);
            }

            logFilePath = Path.Combine(logFilePath, $"servico_log_{DateTime.Now:yyyyMMdd}.txt");

            // Iniciar o log
            LogToFile("Serviço iniciado", true);
            LogToFile($"Modo de log: {(isDebugMode ? "DEBUG" : "NORMAL")}", true);
        }

        protected override void OnStart(string[] args)
        {
            string mensagem = "Serviço iniciado";
            EventLog.WriteEntry(nomeCliente, "start", EventLogEntryType.Warning);
            LogToFile(mensagem, true);
            timer = new Timer(new TimerCallback(Execute), null, 0, 60000);
        }

        protected override void OnStop()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
            string mensagem = "Serviço parado";
            EventLog.WriteEntry(nomeCliente, "stop", EventLogEntryType.Warning);
            LogToFile(mensagem, true);
        }

        private void Execute(object state)
        {
            LogToFile("Iniciando execução da tarefa programada", false);

            // Obter o diretório pai do executável atual
            string diretorioServico = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string diretorioPai = Directory.GetParent(diretorioServico).FullName;

            var pathIntegracao = Path.Combine(diretorioPai, "integracao");
            var exeIntegracao = "Poliview.crm.integracao.exe";

            var pathEmail = Path.Combine(diretorioPai, "email");
            var exeEmail = "Poliview.crm.service.email.exe";

            var pathMonitor = Path.Combine(diretorioPai, "monitor");
            var exeMonitor = "Poliview.crm.monitor.service.exe";

            var pathSla = Path.Combine(diretorioPai, "sla");
            var exeSla = "poliview.crm.sla.exe";

            LogToFile($"Diretório do serviço: {diretorioServico}", false);
            LogToFile($"Diretório pai: {diretorioPai}", false);
            LogToFile($"Caminho de integração: {pathIntegracao}", false);
            LogToFile($"Caminho de email: {pathEmail}", false);
            LogToFile($"Caminho de monitor: {pathMonitor}", false);
            LogToFile($"Caminho de sla: {pathSla}", false);

            outputBuffer.Clear();
            errorBuffer.Clear();

            var startInfoIntegracao = new ProcessStartInfo
            {
                FileName = $@"{pathIntegracao}\{exeIntegracao}",
                WorkingDirectory = $"{pathIntegracao}"
            };

            var startInfoEmail = new ProcessStartInfo
            {
                FileName = $@"{pathEmail}\{exeEmail}",
                WorkingDirectory = $"{pathEmail}"
            };

            var startInfoMonitor = new ProcessStartInfo
            {
                FileName = $@"{pathMonitor}\{exeMonitor}",
                WorkingDirectory = $"{pathMonitor}"
            };

            var startInfoSla = new ProcessStartInfo
            {
                FileName = $@"{pathSla}\{exeSla}",
                WorkingDirectory = $"{pathSla}"
            };

            LogToFile("Configurando processo INTEGRACAO " + startInfoIntegracao.FileName, false);
            LogToFile("Configurando processo EMAIL " + startInfoEmail.FileName, false);
            LogToFile("Configurando processo MONITOR " + startInfoMonitor.FileName, false);
            LogToFile("Configurando processo SLA " + startInfoSla.FileName, false);

            // Iniciar processo de integração
            try
            {
                var processIntegracao = Process.Start(startInfoIntegracao);
                int pidIntegracao = processIntegracao.Id;
                string mensagemIntegracao = $"Iniciado processo INTEGRACAO {pidIntegracao} - {startInfoIntegracao.FileName}";
                LogEventoInfo(mensagemIntegracao);
            }
            catch (Exception ex)
            {
                string mensagemErro = $"Erro ao iniciar processo INTEGRACAO: {ex.Message}";
                LogEventoErro(mensagemErro);
            }

            // Iniciar processo de email
            try
            {
                var processEmail = Process.Start(startInfoEmail);
                int pidEmail = processEmail.Id;
                string mensagemEmail = $"Iniciado processo EMAIL {pidEmail} - {startInfoEmail.FileName}";
                LogEventoInfo(mensagemEmail);
            }
            catch (Exception ex)
            {
                string mensagemErro = $"Erro ao iniciar processo EMAIL: {ex.Message}";
                LogEventoErro(mensagemErro);
            }

            // Iniciar processo de monitor
            try
            {
                var processMonitor = Process.Start(startInfoMonitor);
                int pidMonitor = processMonitor.Id;
                string mensagemMonitor = $"Iniciado processo MONITOR {pidMonitor} - {startInfoMonitor.FileName}";
                LogEventoInfo(mensagemMonitor);
            }
            catch (Exception ex)
            {
                string mensagemErro = $"Erro ao iniciar processo MONITOR: {ex.Message}";
                LogEventoErro(mensagemErro);
            }

            // Iniciar processo de SLA
            try
            {
                var processSla = Process.Start(startInfoSla);
                int pidSla = processSla.Id;
                string mensagemSla = $"Iniciado processo SLA {pidSla} - {startInfoSla.FileName}";
                LogEventoInfo(mensagemSla);
            }
            catch (Exception ex)
            {
                string mensagemErro = $"Erro ao iniciar processo SLA: {ex.Message}";
                LogEventoErro(mensagemErro);
            }

            // Limpar recursos
            startInfoIntegracao = null;
            startInfoEmail = null;
            startInfoMonitor = null;
            startInfoSla = null;
            LogToFile("Finalizada execução da tarefa programada", false);
        }

        private StringBuilder outputBuffer = new StringBuilder();
        private StringBuilder errorBuffer = new StringBuilder();

        private void ProcessOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                outputBuffer.AppendLine(e.Data);
                LogToFile($"Output: {e.Data}", false);
            }
        }

        private void ProcessErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                errorBuffer.AppendLine(e.Data);
                LogToFile($"Erro: {e.Data}", true);
            }
        }

        /// <summary>
        /// Escreve uma mensagem de informação no log e no EventLog (em modo Debug)
        /// </summary>
        private void LogEventoInfo(string mensagem)
        {
            LogToFile(mensagem, false);
            if (isDebugMode)
            {
                EventLog.WriteEntry(nomeCliente, mensagem, EventLogEntryType.Information);
            }
        }

        /// <summary>
        /// Escreve uma mensagem de erro no log e no EventLog (sempre)
        /// </summary>
        private void LogEventoErro(string mensagem)
        {
            LogToFile(mensagem, true);
            EventLog.WriteEntry(nomeCliente, mensagem, EventLogEntryType.Error);
        }

        /// <summary>
        /// Escreve uma mensagem no arquivo de log
        /// </summary>
        /// <param name="mensagem">Mensagem a ser gravada no log</param>
        /// <param name="isError">Indica se é uma mensagem de erro (sempre registra) ou não (registra apenas em modo Debug)</param>
        private void LogToFile(string mensagem, bool isError)
        {
            // Se não for erro e não estiver em modo Debug, não registra
            if (!isError && !isDebugMode)
                return;

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    string tipoLog = isError ? "[ERRO]" : "[INFO]";
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {tipoLog} - {mensagem}");
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(nomeCliente, $"Erro ao escrever no log: {ex.Message}", EventLogEntryType.Error);
            }
        }
    }
}
