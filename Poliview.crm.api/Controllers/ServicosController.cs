using Microsoft.AspNetCore.Mvc;
using Poliview.crm.services;
using System.Diagnostics;
using System.Text;

namespace Poliview.crm.api.Controllers
{
    [Route("servicos")]
    [ApiController]
    public class ServicosController : ControllerBase
    {
        private readonly string _connectionString;

        public ServicosController(IConfiguration configuration)
        {
            _connectionString = configuration["conexao"];
        }

        /// <summary>Executa o serviço de e-mail (Poliview.crm.service.email.exe) e retorna o console output.</summary>
        [HttpPost("email/executar")]
        public async Task<IActionResult> ExecutarServicoEmail()
        {
            try
            {
                // Obter PastaInstalacaoCRM da tabela OPE_PARAMETRO
                var parametros = ParametrosService.consultar(_connectionString);
                if (string.IsNullOrWhiteSpace(parametros.PastaInstalacaoCRM))
                {
                    return BadRequest(new { error = "Pasta de instalação do CRM (PastaInstalacaoCRM) não configurada na tabela OPE_PARAMETRO." });
                }

                // Construir o caminho do executável
                var caminhoExe = Path.Combine(parametros.PastaInstalacaoCRM, "Servicos", "Email", "Poliview.crm.service.email.exe");

                if (!System.IO.File.Exists(caminhoExe))
                {
                    return BadRequest(new { error = $"Executável não encontrado no caminho: {caminhoExe}" });
                }

                // Configurar e executar o processo
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = caminhoExe,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(caminhoExe)
                    }
                };

                // Capturar output em tempo real
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Aguardar conclusão (com timeout de 5 minutos)
                var timeoutMinutos = 5;
                var concluido = await Task.Run(() => process.WaitForExit(timeoutMinutos * 60 * 1000));

                if (!concluido)
                {
                    process.Kill();
                    return StatusCode(500, new
                    {
                        error = $"Processo excedeu o tempo limite de {timeoutMinutos} minutos e foi encerrado.",
                        output = outputBuilder.ToString(),
                        stderr = errorBuilder.ToString()
                    });
                }

                var exitCode = process.ExitCode;
                var output = outputBuilder.ToString();
                var stderr = errorBuilder.ToString();

                return Ok(new
                {
                    exitCode,
                    output,
                    stderr,
                    caminhoExecutavel = caminhoExe,
                    sucesso = exitCode == 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Erro ao executar serviço: {ex.Message}", stackTrace = ex.StackTrace });
            }
        }
    }
}
