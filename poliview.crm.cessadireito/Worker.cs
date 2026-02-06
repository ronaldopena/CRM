using poliview.crm.cessadireito.Services;

namespace poliview.crm.cessadireito
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly LogService _logService;
        private readonly string? _connectionString;
        private readonly string? _connectionStringFB;
        private IConfiguration _configuration;
        private readonly IHostApplicationLifetime _appLifetime;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, LogService logService, IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            _logService = logService;
            _configuration = configuration;
            _connectionString = configuration["conexao"];
            _connectionStringFB = configuration["conexaoFirebird"];
            _appLifetime = appLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTime data = DateTime.Now.AddDays(-10);
            IIntegracao integracaoAtual;

            await _logService.Log(LogLevel.Information, "*** INICIO PROCESSAMENTO ***");
            //while (!stoppingToken.IsCancellationRequested)
            //{
                integracaoAtual = new IntegracaoProponentes(data, _connectionString, _connectionStringFB, _logService);
                integracaoAtual.Integrar();

                integracaoAtual = new IntegracaoContratos(data, _connectionString, _connectionStringFB, _logService);
                integracaoAtual.Integrar();

                _appLifetime.StopApplication();
            // await Task.Delay(5000, stoppingToken);
            //break;
            //}
            await _logService.Log(LogLevel.Information, "*** FIM PROCESSAMENTO ***");

        }
    }
}