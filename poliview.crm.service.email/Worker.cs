using Microsoft.Extensions.Options;
using Poliview.crm.domain;
using Poliview.crm.repositorios;
using poliview.crm.service.email;
using poliview.crm.service.email.Options;
using Poliview.crm.service.email.Services;
using Poliview.crm.service.email;
using Poliview.crm.services;
using Serilog;

namespace poliview.crm.service.email
{
    public class Worker : BackgroundService
    {
        private int countCICLOS = 0;
        private IConfiguration _configuration;
        private Serilog.ILogger logEnviarEmail;
        private Serilog.ILogger logReceberEmail;
        private Serilog.ILogger logTarefa;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly string? _cliente;
        private readonly ILogger<Worker> _logger;
        private readonly LogService _logService;
        private readonly INotificacaoErro _notificacaoErro;
        private readonly IEmailProviderFactory _providerFactory;

        static class TipoAutenticacao
        {
            public const int taPadrao = 0;
            public const int taOffice365 = 1;
            public const int taGmail = 2;
        }

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IHostApplicationLifetime appLifetime, ITelegramService telegramService, LogService logService, INotificacaoErro notificacaoErro, IEmailProviderFactory providerFactory, IOptions<EmailWorkerOptions> options)
        {
            _logger = logger;
            _logService = logService;
            _configuration = configuration;
            _appLifetime = appLifetime;
            _notificacaoErro = notificacaoErro;
            _providerFactory = providerFactory;
            _cliente = options?.Value?.Cliente ?? configuration["cliente"];
            logEnviarEmail = new LoggerConfiguration()
                                        .MinimumLevel.Debug()
                                        .WriteTo.Console()
                                        .WriteTo.File("log.enviarEmail.txt", rollingInterval: RollingInterval.Day)
                                        .CreateLogger();

            logReceberEmail = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("log.receberEmail.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            logTarefa = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("log.tarefa.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

        }

        protected void printInfo(ContaEmail conta)
        {

            if (conta.tipoconta == 1)
            {
                logEnviarEmail.Debug($"CONFIGURAÇÃO OFFICE365 - {conta.descricaoConta}");
                logEnviarEmail.Debug($"TenantId: {conta.tenant_id}");
                logEnviarEmail.Debug($"ClientId: {conta.client_id}");
                logEnviarEmail.Debug($"UserId: {conta.userId}");

            }
            else if (conta.tipoconta == 2)
            {
                logEnviarEmail.Debug($"CONFIGURAÇÃO GMAIL - {conta.descricaoConta}");
                logEnviarEmail.Debug($"Email: {conta.emailRemetente}");
                logEnviarEmail.Debug($"ClientId: {conta.client_id}");
                logEnviarEmail.Debug($"ClientSecret: {conta.clientSecret}");
            }
            else
            {
                logEnviarEmail.Debug($"CONFIGURAÇÃO POP/SMTP - {conta.descricaoConta}");
                logEnviarEmail.Debug($"Email: {conta.emailRemetente}");
                logEnviarEmail.Debug($"pop: {conta.hostpop}");
                logEnviarEmail.Debug($"smtp: {conta.hostsmtp}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // throw new NotImplementedException("Método ExecuteAsync não implementado.");

                var emailService = new EmailService(_configuration, logTarefa);
                emailService.ConfigurandoRecebimentoEnvioEmails();

                var contasemailservice = new ContaEmailService(_configuration);

                contasemailservice.Reset();

                var retorno = contasemailservice.Listar();
                var contasemail = retorno.objeto;

                foreach (var conta in contasemail)
                {
                    printInfo(conta);
                }

                var count = Interlocked.Increment(ref countCICLOS);
                await ENVIAEMAIL(countCICLOS);
                await RECEBEEMAIL(countCICLOS);
                emailService.SalvarDataHoraUltimoProcessamentoEmail();
            }
            catch (Exception ex)
            {
                var mensagemErro = $"Erro no envio de email do cliente: {_cliente ?? "Não identificado"}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro("Erro no envio de e-mail", mensagemErro);
            }
            finally
            {
                StopAsync(stoppingToken);
                _appLifetime.StopApplication();
            }
        }

        private async Task ENVIAEMAIL(int ciclo)
        {
            try
            {
                logReceberEmail.Information($"ENVIAR EMAIL ciclo: {ciclo}");
                var contasemailservice = new ContaEmailService(_configuration);
                var retorno = contasemailservice.Listar();
                var contasemail = retorno.objeto;

                if (retorno.sucesso)
                {
                    foreach (var conta in contasemail)
                    {
                        if (conta.enviaremail)
                        {
                            logReceberEmail.Debug($"{conta.descricaoConta} - NOVO CICLO DE ENVIO DE EMAIL");
                            var sendEmailObj = new EnviarEmail(_configuration, _logService, _notificacaoErro, _providerFactory);
                            sendEmailObj.tipoAutenticacao = conta.tipoconta;
                            await sendEmailObj.Send(logEnviarEmail, conta);
                        }
                        else
                        {
                            if (!conta.enviaremail) logEnviarEmail.Warning($"{conta.descricaoConta} - ENVIO DE EMAILS DESABILITADO");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logReceberEmail.Error($" {ex.Message} ERRO AO ENVIAR EMAILS ");
                var mensagemErro = $"Erro no envio de emails - Cliente: {_cliente ?? "Não identificado"}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro("Erro no envio de e-mail", mensagemErro);
            }
        }

        private async Task RECEBEEMAIL(int ciclo)
        {
            var contaatual = "ANTES DO PROCESSO";
            try
            {
                logReceberEmail.Information($"RECEBER EMAIL Ciclo {ciclo}");
                var contasemailservice = new ContaEmailService(_configuration);
                var retorno = contasemailservice.Listar();
                var contasemail = retorno.objeto;

                if (retorno.sucesso)
                {
                    foreach (var conta in contasemail)
                    {
                        contaatual = conta.descricaoConta;

                        logReceberEmail.Information($"{conta.descricaoConta} - intervalo {conta.intervalorecebimento}");
                        if (conta.receberemail)
                        {
                            logReceberEmail.Debug($"{conta.descricaoConta} - NOVO CICLO DE RECEBIMENTO DE EMAIL");
                            var receiveEmailObj = new ReceberEmail(_configuration, _logService, _notificacaoErro, _providerFactory);
                            receiveEmailObj.tipoAutenticacao = conta.tipoconta;
                            await receiveEmailObj.receiveAsync(conta, logReceberEmail);
                        }
                        else
                        {
                            if (!conta.receberemail) logReceberEmail.Warning($" {conta.descricaoConta} - RECEBIMENTO DE EMAILS DESABILITADO");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logReceberEmail.Error($"{contaatual} - {ex.Message} ERRO AO RECEBER EMAILS ");
                var mensagemErro = $"Erro no recebimento de emails - Cliente: {_cliente ?? "Não identificado"}\n\n" +
                                 $"Conta: {contaatual}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro("Erro no recebimento de e-mail", mensagemErro);
            }

        }

        public override void Dispose()
        {
            Console.WriteLine("Worker foi descartado.");
            base.Dispose();
        }
    }
}