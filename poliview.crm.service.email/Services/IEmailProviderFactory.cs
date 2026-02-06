using Poliview.crm.domain;

namespace Poliview.crm.service.email.Services;

/// <summary>
/// Factory para obter o serviço de envio/recebimento conforme o tipo da conta.
/// Elimina switch + new nos orquestradores e permite mock em testes.
/// </summary>
public interface IEmailProviderFactory
{
    poliview.crm.service.email.Services.ISendEmailService GetSendService(int tipoConta);
    poliview.crm.service.email.Services.IReceiveEmailService GetReceiveService(int tipoConta);
}
