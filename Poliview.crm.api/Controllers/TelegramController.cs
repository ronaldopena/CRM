using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Poliview.crm.models;
using Poliview.crm.services;

namespace Poliview.crm.api.Controllers
{
    [Route("telegram")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        private readonly ITelegramService _telegramService;

        public TelegramController(ITelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        /// <summary>
        /// Envia uma mensagem de texto via Telegram
        /// </summary>
        /// <param name="request">Dados da mensagem a ser enviada</param>
        /// <returns>Resultado do envio</returns>
        [Authorize]
        [HttpPost("enviar-mensagem")]
        public async Task<IActionResult> EnviarMensagem([FromBody] EnviarMensagemRequest request)
        {
            var resultado = await _telegramService.EnviarMensagemAsync(
                request.Mensagem,
                request.ChatId,
                request.FormatMarkdown
            );

            if (resultado.Sucesso)
            {
                return Ok(resultado);
            }

            return BadRequest(resultado);
        }

        /// <summary>
        /// Envia um arquivo via Telegram
        /// </summary>
        /// <param name="request">Dados do arquivo a ser enviado</param>
        /// <returns>Resultado do envio</returns>
        [HttpPost("enviar-arquivo")]
        public async Task<IActionResult> EnviarArquivo([FromBody] EnviarArquivoRequest request)
        {
            var resultado = await _telegramService.EnviarArquivoAsync(
                request.CaminhoArquivo,
                request.Legenda,
                request.ChatId
            );

            if (resultado.Sucesso)
            {
                return Ok(resultado);
            }

            return resultado.Mensagem.Contains("Erro ao")
                ? StatusCode(500, resultado)
                : BadRequest(resultado);
        }

        /// <summary>
        /// Obtém informações sobre o bot
        /// </summary>
        /// <returns>Informações do bot</returns>
        [HttpGet("info-bot")]
        public async Task<IActionResult> ObterInfoBot()
        {
            var resultado = await _telegramService.ObterInfoBotAsync();

            if (resultado.Sucesso)
            {
                return Ok(resultado);
            }

            return StatusCode(500, resultado);
        }

        /// <summary>
        /// Testa a conexão com o bot do Telegram
        /// </summary>
        /// <returns>Status da conexão</returns>
        [HttpGet("testar-conexao")]
        public async Task<IActionResult> TestarConexao()
        {
            var resultado = await _telegramService.TestarConexaoAsync();

            if (resultado.Sucesso)
            {
                return Ok(resultado);
            }

            return StatusCode(500, resultado);
        }

        /// <summary>
        /// Envia uma mensagem formatada com informações do sistema
        /// </summary>
        /// <param name="titulo">Título da notificação</param>
        /// <param name="mensagem">Mensagem principal</param>
        /// <param name="nivel">Nível da mensagem (Info, Warning, Error)</param>
        /// <param name="chatId">Chat ID opcional</param>
        /// <returns>Resultado do envio</returns>
        [HttpPost("notificacao-sistema")]
        public async Task<IActionResult> EnviarNotificacaoSistema(
            [FromQuery] string titulo,
            [FromQuery] string mensagem,
            [FromQuery] string nivel = "Info",
            [FromQuery] string? chatId = null)
        {
            var resultado = await _telegramService.EnviarNotificacaoSistemaAsync(
                titulo,
                mensagem,
                nivel,
                chatId
            );

            if (resultado.Sucesso)
            {
                return Ok(resultado);
            }

            return resultado.Mensagem.Contains("Erro ao")
                ? StatusCode(500, resultado)
                : BadRequest(resultado);
        }
    }
}