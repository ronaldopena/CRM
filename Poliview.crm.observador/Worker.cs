using Poliview.crm.http.services;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.observador
{
    public class Worker : BackgroundService
    {
        // private readonly ILogger<Worker> _logger;
        private readonly HttpClient _httpclient;
        private readonly IConfiguration _configuration;

        public Worker(HttpClient httpclient, IConfiguration configuration)
        {
            //_logger = logger;
            _httpclient = httpclient;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var service = new AutenticacaoHttpService(_httpclient);
            var utilService = new UtilsService(_configuration);

            while (!stoppingToken.IsCancellationRequested)
            {
                var obj = new LoginRequisicao();
                obj.usuario = "usuario@empresa.com.br";
                // obj.senha = "1234";
                obj.senha = "@PoliCrmSac";
                obj.idempresa = 1;
                obj.origem = 1;

                var result = await service.Login(obj);

                // var result = await new UsuarioHttpService(_httpclient).Login(obj);

                if (result.sucesso)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} Acesso Ok");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} Erro acesso: {result.mensagem}");
                    utilService.enviarEmailSuporte("Erro de acesso api da gencons!", $"{obj.usuario} - {result.mensagem}");
                }
                
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}