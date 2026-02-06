namespace Poliview.crm.models
{
    public class EnviarMensagemRequest
    {
        public string Mensagem { get; set; } = string.Empty;
        public string? ChatId { get; set; }
        public bool FormatMarkdown { get; set; } = false;
    }

    public class EnviarArquivoRequest
    {
        public string CaminhoArquivo { get; set; } = string.Empty;
        public string? Legenda { get; set; }
        public string? ChatId { get; set; }
    }

    public class TelegramResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public object? Dados { get; set; }
    }
}