using IntegracaoMobussService.repositorio;
using IntegracaoMobussService.Repositorio;
using IntegracaoMobussService.Services;
using static IntegracaoMobussService.Services.ServiceConexao;

namespace Poliview.crm.mobuss.integracao
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private string _connectionString;
        private int ciclo = 0;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = new Conexao(configuration).StringConexao;
            ciclo = configuration.GetValue<int>("ciclointegracao");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var objChamados = new ChamadoMobussRepositorio(_connectionString);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Integração Mobuss: {time}", DateTimeOffset.Now);

                    var objConfigCrm = new ConfiguracaoCrmRepositorio(_connectionString);
                    var configCrm = objConfigCrm.Config();

                    if (configCrm.integracaoMobuss == 1)
                    {
                        objChamados.PrepararIntegracao();

                        var Chamados = objChamados.Listar(configCrm.idOcorrenciaRaizIntegracaoMobuss,
                                                          configCrm.StatusChamadoEmAtendimento);
                        var http = new ServiceHttp();

                        foreach (var chamado in Chamados)
                        {
                            var txt = $"Nome: {chamado.nomeSolicitante} Chamado: {chamado.numSolicitacao} idLocal: {chamado.idLocal}";

                            try
                            {
                                _logger.LogInformation(txt);
                                var resultado = await http.EnviarChamadoAsync(chamado, configCrm);
                                // var resultado = "ERRO FORÇADO";
                                objChamados.AlterarStatusChamadoMobuss(chamado.numSolicitacao, resultado != "" ? 9 : 1, resultado);
                                _logger.LogInformation(resultado);

                                if (resultado != "")
                                {
                                    objChamados.Log(9, resultado + " - " + txt, chamado.numSolicitacao);
                                }
                                else
                                {
                                    objChamados.Log(1, "OK! - " + txt, chamado.numSolicitacao);
                                }
                            }
                            catch (Exception ex)
                            {
                                objChamados.Log(9, ex.Message + " - " + txt, chamado.numSolicitacao);
                            }
                        }
                    }
                    else
                    {
                        objChamados.Log(2, "Integração Mobuss não habilitada!");
                        _logger.LogInformation("Integração Mobuss não habilitada!");
                    }

                    await Task.Delay(ciclo * 1000, stoppingToken);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in background service: {ex.Message}");
                }
            }
        }
    }
}