using Poliview.crm.models;

namespace Poliview.crm.services
{
    public interface ITelegramService
    {
        Task<TelegramResponse> EnviarMensagemAsync(string mensagem, string? chatId = null, bool formatMarkdown = false);
        Task<TelegramResponse> EnviarArquivoAsync(string caminhoArquivo, string? legenda = null, string? chatId = null);
        Task<TelegramResponse> ObterInfoBotAsync();
        Task<TelegramResponse> TestarConexaoAsync();
        Task<TelegramResponse> EnviarNotificacaoSistemaAsync(string titulo, string mensagem, string nivel = "Info", string? chatId = null);
    }
}