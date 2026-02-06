namespace Poliview.crm.service.email.Services;

/// <summary>
/// Abstração para notificação de erro (ex.: Telegram + log).
/// Permite mock em testes e centraliza o tratamento.
/// </summary>
public interface INotificacaoErro
{
    void NotificarErro(string titulo, string mensagem);
}
