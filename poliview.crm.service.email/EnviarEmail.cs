using Poliview.crm.domain;
using Poliview.crm.services;
using Poliview.crm.service.email.Services;
using Poliview.crm.repositorios;

namespace poliview.crm.service.email
{
    public class EnviarEmail
    {
        private IConfiguration _configuration;
        private LogService _logService;
        private string _cliente = "não identificado";
        private static string _tituloMensagem = "Envio de E-mail";
        private string? tipoAutenticacaoText { get; set; }
        private string connection { get; set; }
        public int tipoAutenticacao { get; set; }
        public string emailalternativo { get; set; }
        private string[] descrTipoAutenticacao = { "POP/SMTP", "OFFICE365", "GMAIL" };

        public EnviarEmail(IConfiguration configuration, LogService logService)
        {
            _configuration = configuration;
            _logService = logService;
            _cliente = configuration["cliente"] ?? "não identificado";
            connection = configuration["conexao"].ToString();
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
                log.Debug($"{conta.descricaoConta} - Tipo Autenticação: {tipoAutenticacao}-{descrTipoAutenticacao[tipoAutenticacao]}");
                poliview.crm.service.email.Services.ISendEmailService service;

                switch (tipoAutenticacao)
                {
                    case 0:
                        service = new SendEmailPadraoService(_configuration, _logService);
                        break;
                    case 1:
                        service = new SendEmailOffice365Service(_configuration, _logService);
                        break;
                    case 2:
                        service = new SendEmailGmailService(_configuration, _logService);
                        break;
                    default: throw new System.Exception("Tipo de autenticação inválidad!");
                }

                service.SendEmailAsync(EmailsParaEnviar, conta, log);
            }
            catch (Exception ex)
            {
                log.Error($"{conta.descricaoConta} - ERRO NO ENVIO DE EMAIL: {ex.Message}");

                // Enviar notificação via Telegram
                try
                {
                    var mensagemErro = $"Erro no envio de email - Conta: {conta.descricaoConta}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                    UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                }
                catch (Exception telegramEx)
                {
                    await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                        $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                }
            }
        }

        public void EnviarEmailAvulso(string destinatarios, string assunto, string corpo, Serilog.ILogger log)
        {
            try
            {
                var contasemailservice = new ContaEmailService(_configuration);
                var retorno = contasemailservice.Listar();
                var conta = retorno.objeto.First();

                tipoAutenticacao = conta.tipoconta;
                var emailService = new EmailService(_configuration, log);
                poliview.crm.service.email.Services.ISendEmailService service;

                switch (tipoAutenticacao)
                {
                    case 0:
                        service = new SendEmailPadraoService(_configuration, _logService);
                        break;
                    case 1:
                        service = new SendEmailOffice365Service(_configuration, _logService);
                        break;
                    case 2:
                        service = new SendEmailGmailService(_configuration, _logService);
                        break;
                    default: throw new System.Exception("Tipo de autenticação inválida!");
                }

                // service.EnviarEmailAvulso(destinatarios, assunto, corpo, conta, log);
            }
            catch (Exception ex)
            {
                log.Error($"ERRO NO ENVIO DE EMAIL AVULSO: {ex.Message}");

                // Enviar notificação via Telegram
                try
                {
                    var mensagemErro = $"Erro no envio de email avulso\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                    UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, _configuration);
                }
                catch (Exception telegramEx)
                {
                    _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                        $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                }
            }
        }

    }
}
