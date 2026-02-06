using Microsoft.Extensions.Hosting;
using poliview.crm.integracao;
using Poliview.crm.repositorios;
using Poliview.crm.services;
using System.Diagnostics;

namespace Poliview.crm.integracao
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly LogService _logService;
        private readonly string? _connectionString;
        private readonly string? _connectionStringFB;
        private IConfiguration _configuration;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly string? _cliente;
        private static string _tituloMensagem = "RAIZ DA INTEGRAÇÃO";

        public Worker(ILogger<Worker> logger,
                      IConfiguration configuration,
                      LogService logService,
                      IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _logService = logService;
            _configuration = configuration;
            _connectionString = configuration["conexao"];
            _connectionStringFB = configuration["conexaoFirebird"];
            _appLifetime = appLifetime;
            _cliente = configuration["cliente"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var integracaoService = new IntegracaoService(_configuration);
            try
            {

                IIntegracao integracaoAtual;
                DateTime datahoraatual = DateTime.Now;
                var config = integracaoService.config();
                var dataultimaintegracao = config.DataUltimaIntegracao;
                // var dataultimaintegracao = Convert.ToDateTime("2000-01-01 00:00:00");
                var inicio = integracaoService.Inicio();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var service = new UtilsService(_configuration);

                service.ExcluirRegistrosChaveNula();

                await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.info, "*** INICIO PROCESSAMENTO INTEGRACAO ***");
                // inicio = false;

                if (inicio)
                {
                    // empreendimentos
                    integracaoAtual = new IntegracaoEmpreendimentos(dataultimaintegracao, datahoraatual, 3, _connectionString, _connectionStringFB, _logService, _configuration);
                    integracaoAtual.Integrar();
                    // blocos
                    integracaoAtual = new IntegracaoBlocos(dataultimaintegracao, datahoraatual, 4, _connectionString, _connectionStringFB, _logService, _configuration);
                    integracaoAtual.Integrar();
                    // Tipo Unidade
                    integracaoAtual = new IntegracaoTipoUnidade(dataultimaintegracao, datahoraatual, 57, _connectionString, _connectionStringFB, _logService, _configuration);
                    integracaoAtual.Integrar();
                    // unidades
                    integracaoAtual = new IntegracaoUnidades(dataultimaintegracao, datahoraatual, 5, _connectionString, _connectionStringFB, _logService, _configuration);
                    integracaoAtual.Integrar();
                    // clientes
                    integracaoAtual = new IntegracaoClientes(dataultimaintegracao, datahoraatual, 1, _connectionString, _connectionStringFB, _logService, _configuration);
                    integracaoAtual.Integrar();
                    // contratos
                    integracaoAtual = new IntegracaoContratos(dataultimaintegracao, datahoraatual, 2, _connectionString, _connectionStringFB, _logService, _configuration);
                    integracaoAtual.Integrar();
                    // proponentes
                    integracaoAtual = new IntegracaoProponentes(dataultimaintegracao, datahoraatual, 56, _connectionString, _connectionStringFB, _logService, _configuration);
                    integracaoAtual.Integrar();

                    integracaoService.Fim();
                    integracaoService.AlterarDataHoraUltimaIntegracao(datahoraatual);
                }
                else
                {
                    await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.aviso, "J� EXISTE UMA INTEGRA��O EM ANDAMENTO");
                }
                // _appLifetime.StopApplication();
                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
                await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.aviso, $"*** FIM PROCESSAMENTO INTEGRACAO *** Tempo decorrido: {elapsedTime}");
                // await _telegramService.EnviarNotificacaoSistemaAsync(
                //        $"CLIENTE: {_cliente ?? "Não identificado"}",
                //        $"*** FIM PROCESSAMENTO INTEGRACAO *** Tempo decorrido: {elapsedTime}",
                //        "WARNING"
                // );
            }
            catch (Exception ex)
            {
                integracaoService.Fim();
                await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro, $"ERRO: {ex.Message}");

                // Enviar notificação via Telegram
                try
                {
                    var mensagemErro = $"Erro na integração do cliente: {_cliente ?? "Não identificado"}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                    IntegracaoUtil.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                }
                catch (Exception telegramEx)
                {
                    await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                        $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                }
            }
            finally
            {
                integracaoService.Fim();
                StopAsync(stoppingToken);
                _appLifetime.StopApplication();
            }

            // await Task.Delay(60000, stoppingToken);
            //}

        }

        public override void Dispose()
        {
            Console.WriteLine("Worker foi descartado.");
            base.Dispose();
        }
    }
}