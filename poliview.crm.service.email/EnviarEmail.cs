using Poliview.crm.domain;
using Poliview.crm.services;
using Poliview.crm.service.email.Services;
using Poliview.crm.repositorios;

namespace poliview.crm.service.email
{
    public class EnviarEmail
    {
        private readonly IConfiguration _configuration;
        private readonly LogService _logService;
        private readonly INotificacaoErro _notificacaoErro;
        private readonly IEmailProviderFactory _providerFactory;
        private readonly string _cliente;
        private const string TituloMensagem = "Envio de E-mail";
        private string? tipoAutenticacaoText { get; set; }
        private readonly string connection;
        public int tipoAutenticacao { get; set; }
        public string emailalternativo { get; set; } = "";
        private static readonly string[] DescrTipoAutenticacao = { "POP/SMTP", "OFFICE365", "GMAIL" };

        public EnviarEmail(IConfiguration configuration, LogService logService, INotificacaoErro notificacaoErro, IEmailProviderFactory providerFactory)
        {
            _configuration = configuration;
            _logService = logService;
            _notificacaoErro = notificacaoErro;
            _providerFactory = providerFactory;
            _cliente = configuration["cliente"] ?? "não identificado";
            connection = configuration["conexao"]?.ToString() ?? "";
        }

        public async Task Send(Serilog.ILogger log, ContaEmail conta)
        {
            try
            {
                log.Debug($"{conta.descricaoConta} - METODO SEND");
                tipoAutenticacao = conta.tipoconta;
                var emailService = new EmailService(_configuration, log);
                var EmailsParaEnviar = emailService.PendentesParaEnvio(log, conta);
                log.Debug($"{conta.descricaoConta} - total de emails para enviar: {EmailsParaEnviar.Count} ");
                log.Debug($"{conta.descricaoConta} - Tipo Autenticação: {tipoAutenticacao}-{DescrTipoAutenticacao[tipoAutenticacao]}");
                var service = _providerFactory.GetSendService(tipoAutenticacao);
                service.SendEmailAsync(EmailsParaEnviar, conta, log);
            }
            catch (Exception ex)
            {
                log.Error($"{conta.descricaoConta} - ERRO NO ENVIO DE EMAIL: {ex.Message}");
                var mensagemErro = $"Erro no envio de email - Conta: {conta.descricaoConta}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(TituloMensagem, mensagemErro);
            }
        }

        public async Task EnviarEmailAvulsoAsync(string destinatarios, string assunto, string corpo, Serilog.ILogger log)
        {
            try
            {
                var contasemailservice = new ContaEmailService(_configuration);
                var retorno = contasemailservice.Listar();
                var conta = retorno.objeto.First();

                tipoAutenticacao = conta.tipoconta;
                var service = _providerFactory.GetSendService(tipoAutenticacao);
                await service.EnviarEmailAvulsoAsync(destinatarios, assunto, corpo, conta, log);
            }
            catch (Exception ex)
            {
                log.Error($"ERRO NO ENVIO DE EMAIL AVULSO: {ex.Message}");
                var mensagemErro = $"Erro no envio de email avulso\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(TituloMensagem, mensagemErro);
            }
        }

    }
}
