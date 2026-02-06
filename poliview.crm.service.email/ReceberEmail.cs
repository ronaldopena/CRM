using Poliview.crm.domain;
using poliview.crm.service.email.Services;
using Poliview.crm.services;
using static ClosedXML.Excel.XLPredefinedFormat;
using Poliview.crm.service.email.Services;
using System.Runtime.InteropServices;
using Poliview.crm.repositorios;

namespace Poliview.crm.service.email
{
    public class ReceberEmail
    {
        private IConfiguration configuration;
        private LogService _logService;
        private string _cliente = "não identificado";
        private static string _tituloMensagem = "Recebimento de E-mail";
        public int tipoAutenticacao { get; set; }
        private string connectionString { get; set; }

        public ReceberEmail(IConfiguration _configuration, LogService logService)
        {
            configuration = _configuration;
            _logService = logService;
            _cliente = configuration["cliente"] ?? "não identificado";
            connectionString = configuration["Conexao"].ToString();
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
                        IReceiveEmailService service = null;

                        switch (tipoAutenticacao)
                        {
                            case 0:
                                service = new ReceiveEmailPadraoService(configuration, _logService);
                                break;
                            case 1:
                                service = new ReceiveEmailOffice365Service(configuration, _logService);
                                break;
                            case 2:
                                service = new ReceiveEmailGmailService(configuration, _logService);
                                break;
                            default: throw new System.Exception("Tipo de autenticação inválida!");
                        }

                        if (service != null) await service.ReceiveEmailAsync(log, conta);
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
                log.Error($"{conta.descricaoConta} - ReceberEmail.cs(55) - " + ex.Message);

                // Enviar notificação via Telegram
                try
                {
                    var mensagemErro = $"Erro no recebimento de email - Conta: {conta.descricaoConta}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                    UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);
                }
                catch (Exception telegramEx)
                {
                    await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                        $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                }
            }
        }
    }
}
