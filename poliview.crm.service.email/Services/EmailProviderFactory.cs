using Poliview.crm.domain;
using Poliview.crm.services;
using poliview.crm.service.email;

namespace Poliview.crm.service.email.Services;

/// <summary>
/// Implementação da factory que instancia o serviço de envio/recebimento conforme tipo da conta.
/// </summary>
public class EmailProviderFactory : IEmailProviderFactory
{
    private readonly IConfiguration _configuration;
    private readonly LogService _logService;
    private readonly INotificacaoErro _notificacaoErro;

    public EmailProviderFactory(IConfiguration configuration, LogService logService, INotificacaoErro notificacaoErro)
    {
        _configuration = configuration;
        _logService = logService;
        _notificacaoErro = notificacaoErro;
    }

    public poliview.crm.service.email.Services.ISendEmailService GetSendService(int tipoConta)
    {
        return tipoConta switch
        {
            (int)TipoContaEmail.Padrao => new SendEmailPadraoService(_configuration, _logService, _notificacaoErro),
            (int)TipoContaEmail.Office365 => new SendEmailOffice365Service(_configuration, _logService, _notificacaoErro),
            (int)TipoContaEmail.Gmail => new SendEmailGmailService(_configuration, _logService, _notificacaoErro),
            _ => throw new ArgumentException($"Tipo de autenticação inválido: {tipoConta}")
        };
    }

    public poliview.crm.service.email.Services.IReceiveEmailService GetReceiveService(int tipoConta)
    {
        return tipoConta switch
        {
            (int)TipoContaEmail.Padrao => new ReceiveEmailPadraoService(_configuration, _logService, _notificacaoErro),
            (int)TipoContaEmail.Office365 => new ReceiveEmailOffice365Service(_configuration, _logService, _notificacaoErro),
            (int)TipoContaEmail.Gmail => new ReceiveEmailGmailService(_configuration, _logService, _notificacaoErro),
            _ => throw new ArgumentException($"Tipo de autenticação inválido: {tipoConta}")
        };
    }
}
