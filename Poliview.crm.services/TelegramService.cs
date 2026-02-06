using Microsoft.Extensions.Configuration;
using Poliview.crm.models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Poliview.crm.services
{
    public class TelegramService : ITelegramService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IConfiguration _configuration;
        private readonly string _defaultChatId;

        public TelegramService(IConfiguration configuration)
        {
            _configuration = configuration;
            var botToken = _configuration["Telegram:BotToken"];
            _defaultChatId = _configuration["Telegram:ChatId"] ?? "";

            if (string.IsNullOrEmpty(botToken))
            {
                throw new ArgumentException("Token do bot do Telegram não configurado");
            }

            _botClient = new TelegramBotClient(botToken);
        }

        public async Task<TelegramResponse> EnviarMensagemAsync(string mensagem, string? chatId = null, bool formatMarkdown = false)
        {
            try
            {
                if (string.IsNullOrEmpty(mensagem))
                {
                    return new TelegramResponse
                    {
                        Sucesso = false,
                        Mensagem = "Mensagem é obrigatória"
                    };
                }

                var targetChatId = !string.IsNullOrEmpty(chatId) ? chatId : _defaultChatId;

                if (string.IsNullOrEmpty(targetChatId))
                {
                    return new TelegramResponse
                    {
                        Sucesso = false,
                        Mensagem = "Chat ID não configurado"
                    };
                }

                Telegram.Bot.Types.Enums.ParseMode? parseMode = formatMarkdown ? Telegram.Bot.Types.Enums.ParseMode.Markdown : null;

                var message = await _botClient.SendTextMessageAsync(
                    chatId: targetChatId,
                    text: mensagem,
                    parseMode: parseMode
                );

                return new TelegramResponse
                {
                    Sucesso = true,
                    Mensagem = "Mensagem enviada com sucesso",
                    Dados = new
                    {
                        MessageId = message.MessageId,
                        ChatId = message.Chat.Id.ToString(),
                        DataEnvio = message.Date
                    }
                };
            }
            catch (Exception ex)
            {
                return new TelegramResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao enviar mensagem: {ex.Message}"
                };
            }
        }

        public async Task<TelegramResponse> EnviarArquivoAsync(string caminhoArquivo, string? legenda = null, string? chatId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(caminhoArquivo))
                {
                    return new TelegramResponse
                    {
                        Sucesso = false,
                        Mensagem = "Caminho do arquivo é obrigatório"
                    };
                }

                if (!System.IO.File.Exists(caminhoArquivo))
                {
                    return new TelegramResponse
                    {
                        Sucesso = false,
                        Mensagem = "Arquivo não encontrado"
                    };
                }

                var targetChatId = !string.IsNullOrEmpty(chatId) ? chatId : _defaultChatId;

                if (string.IsNullOrEmpty(targetChatId))
                {
                    return new TelegramResponse
                    {
                        Sucesso = false,
                        Mensagem = "Chat ID não configurado"
                    };
                }

                using var stream = System.IO.File.OpenRead(caminhoArquivo);
                var fileName = Path.GetFileName(caminhoArquivo);
                var inputFile = InputFile.FromStream(stream, fileName);

                var message = await _botClient.SendDocumentAsync(
                    chatId: targetChatId,
                    document: inputFile,
                    caption: legenda
                );

                return new TelegramResponse
                {
                    Sucesso = true,
                    Mensagem = "Arquivo enviado com sucesso",
                    Dados = new
                    {
                        MessageId = message.MessageId,
                        ChatId = message.Chat.Id.ToString(),
                        DataEnvio = message.Date,
                        NomeArquivo = fileName
                    }
                };
            }
            catch (Exception ex)
            {
                return new TelegramResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao enviar arquivo: {ex.Message}"
                };
            }
        }

        public async Task<TelegramResponse> ObterInfoBotAsync()
        {
            try
            {
                var me = await _botClient.GetMeAsync();

                return new TelegramResponse
                {
                    Sucesso = true,
                    Mensagem = "Informações do bot obtidas com sucesso",
                    Dados = new
                    {
                        Id = me.Id,
                        Nome = me.FirstName,
                        Username = me.Username,
                        PodeReceberMensagens = me.CanReadAllGroupMessages,
                        SuportaInline = me.SupportsInlineQueries
                    }
                };
            }
            catch (Exception ex)
            {
                return new TelegramResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao obter informações do bot: {ex.Message}"
                };
            }
        }

        public async Task<TelegramResponse> TestarConexaoAsync()
        {
            try
            {
                var me = await _botClient.GetMeAsync();

                return new TelegramResponse
                {
                    Sucesso = true,
                    Mensagem = $"Conexão com o bot '{me.FirstName}' estabelecida com sucesso",
                    Dados = new
                    {
                        BotId = me.Id,
                        BotUsername = me.Username,
                        ChatIdConfigurado = !string.IsNullOrEmpty(_defaultChatId)
                    }
                };
            }
            catch (Exception ex)
            {
                return new TelegramResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro na conexão com o bot: {ex.Message}"
                };
            }
        }

        public async Task<TelegramResponse> EnviarNotificacaoSistemaAsync(string titulo, string mensagem, string nivel = "Info", string? chatId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(titulo) || string.IsNullOrEmpty(mensagem))
                {
                    return new TelegramResponse
                    {
                        Sucesso = false,
                        Mensagem = "Título e mensagem são obrigatórios"
                    };
                }

                var targetChatId = !string.IsNullOrEmpty(chatId) ? chatId : _defaultChatId;

                if (string.IsNullOrEmpty(targetChatId))
                {
                    return new TelegramResponse
                    {
                        Sucesso = false,
                        Mensagem = "Chat ID não configurado"
                    };
                }

                var emoji = nivel.ToUpper() switch
                {
                    "ERROR" => "🚨",
                    "WARNING" => "⚠️",
                    "SUCCESS" => "✅",
                    _ => "ℹ️"
                };

                var mensagemFormatada = $"{emoji} *{titulo}*\n\n" +
                                      $"{mensagem}\n\n" +
                                      $"📅 {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                                      $"🏢 Poliview CRM";

                var message = await _botClient.SendTextMessageAsync(
                    chatId: targetChatId,
                    text: mensagemFormatada,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
                );

                return new TelegramResponse
                {
                    Sucesso = true,
                    Mensagem = "Notificação enviada com sucesso",
                    Dados = new
                    {
                        MessageId = message.MessageId,
                        ChatId = message.Chat.Id.ToString(),
                        DataEnvio = message.Date
                    }
                };
            }
            catch (Exception ex)
            {
                return new TelegramResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao enviar notificação: {ex.Message}"
                };
            }
        }
    }
}