using Poliview.crm.domain;
using Poliview.crm.service.email.Services;
using poliview.crm.service.email.Services;
using Poliview.crm.services;
using Poliview.crm.repositorios;

namespace Poliview.crm.service.email
{
    public class ReceberEmail
    {
        private readonly IConfiguration configuration;
        private readonly LogService _logService;
        private readonly INotificacaoErro _notificacaoErro;
        private readonly IEmailProviderFactory _providerFactory;
        private const string TituloMensagem = "Recebimento de E-mail";
        public int tipoAutenticacao { get; set; }
        private readonly string connectionString;

        public ReceberEmail(IConfiguration config, LogService logService, INotificacaoErro notificacaoErro, IEmailProviderFactory providerFactory)
        {
            configuration = config;
            _logService = logService;
            _notificacaoErro = notificacaoErro;
            _providerFactory = providerFactory;
            connectionString = configuration["Conexao"]?.ToString() ?? "";
        }

        public async Task receiveAsync(ContaEmail conta, Serilog.ILogger log)
        {
            var emailService = new EmailService(configuration, log);
            var recebendoEmails = await emailService.RecebendoEmails(conta.id);

            log.Information($"{conta.descricaoConta} - RECEBENDO EMAILS = {recebendoEmails}");
            try
            {
                if (recebendoEmails == 0)
                {
                    try
                    {
                        await emailService.IniciarReceberEmails(conta.id);
                        tipoAutenticacao = conta.tipoconta;
                        var service = _providerFactory.GetReceiveService(tipoAutenticacao);
                        await service.ReceiveEmailAsync(log, conta);
                    }
                    finally
                    {
                        await emailService.PararReceberEmails(conta.id);
                    }
                }
                else
                {
                    log.Information($"{conta.descricaoConta} - JÁ EXISTE UM PROCESSO DE RECEBIMENTO DE EMAIL ");
                }
            }
            catch (Exception ex)
            {
                log.Error($"{conta.descricaoConta} - ReceberEmail - " + ex.Message);
                var mensagemErro = $"Erro no recebimento de email - Conta: {conta.descricaoConta}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(TituloMensagem, mensagemErro);
            }
        }
    }
}
