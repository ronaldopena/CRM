using FirebirdSql.Data.FirebirdClient;
using Dapper;
using Poliview.crm.services;

namespace poliview.crm.integracao
{
    public static class IntegracaoUtil
    {
        public static void DeletarOrigemExclusaoNulo(FbConnection _connectionFB)
        {
            var connection = _connectionFB;
            var sql = $"DELETE FROM CRM_EXCLUSAO where CHAVE is null";
            connection.ExecuteAsync(sql).Wait();
        }

        public static void NotificarErro(string titulo, string mensagem, IConfiguration configuration)
        {
            var _telegramService = new TelegramService(configuration);
            try
            {
                _telegramService.EnviarNotificacaoSistemaAsync(
                    titulo,
                    mensagem,
                    "ERROR"
                );
            }
            catch (Exception telegramEx)
            {
                //_logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                //    $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
            }
            finally
            {
                _telegramService = null;
            }
        }

    }
}
