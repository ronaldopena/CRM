using Poliview.crm.services;

namespace poliview.crm.sla
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _logger.LogInformation($"Iniciando Serviço de Controle de SLA: ", DateTimeOffset.Now);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            printInfo();
            var sla = new SLAService(_configuration);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Controle SLA: ", DateTimeOffset.Now);
                sla.RecalcularSLA();
				sla.MonitoramentoSLA();
				await Task.Delay(60000, stoppingToken);
            }
        }

        protected void printInfo()
        {            
            this._logger.LogInformation("PRINT INFO");
        }
    }
}