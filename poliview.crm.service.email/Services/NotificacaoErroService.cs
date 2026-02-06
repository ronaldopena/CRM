using Poliview.crm.repositorios;
using Poliview.crm.services;

namespace Poliview.crm.service.email.Services;

/// <summary>
/// Implementação que envia notificação via Telegram e registra falha no log.
/// </summary>
public class NotificacaoErroService : INotificacaoErro
{
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public NotificacaoErroService(IConfiguration configuration, ILogService logService)
    {
        _configuration = configuration;
        _logService = logService;
    }

    public void NotificarErro(string titulo, string mensagem)
    {
        try
        {
            var telegramService = new TelegramService(_configuration);
            telegramService.EnviarNotificacaoSistemaAsync(titulo, mensagem, "ERROR");
        }
        catch (Exception ex)
        {
            _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                $"Erro ao enviar notificação Telegram: {ex.Message}");
        }
    }
}
