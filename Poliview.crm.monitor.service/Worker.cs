using Poliview.crm.infra;
using Poliview.crm.services;

namespace Poliview.crm.monitor.service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IConfiguration _configuration;
        private string connectionstring = "";

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            connectionstring = _configuration["conexao"].ToString();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var parametros = ParametrosService.consultar(connectionstring);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                Util.ExcluirArquivosPDF(parametros.caminhoPdf);
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}