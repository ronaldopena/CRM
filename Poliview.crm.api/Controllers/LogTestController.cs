using Microsoft.AspNetCore.Mvc;

namespace Poliview.crm.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogTestController : ControllerBase
    {
        private readonly ConsoleLogService _consoleLogService;

        public LogTestController(ConsoleLogService consoleLogService)
        {
            _consoleLogService = consoleLogService;
        }

        /// <summary>
        /// Testa o sistema de log escrevendo algumas mensagens no console
        /// </summary>
        [HttpGet("test")]
        public IActionResult TestLog()
        {
            Console.WriteLine("=== TESTE DO SISTEMA DE LOG ===");
            Console.WriteLine($"Timestamp: {DateTime.Now}");
            Console.WriteLine("Esta mensagem deve aparecer no console E no arquivo de log (se habilitado)");
            Console.WriteLine($"Log em arquivo habilitado: {_consoleLogService.IsFileLoggingEnabled}");
            Console.WriteLine($"Caminho do arquivo de log: {_consoleLogService.LogFilePath}");
            Console.WriteLine("=== FIM DO TESTE ===");

            return Ok(new
            {
                Message = "Teste de log executado com sucesso",
                FileLoggingEnabled = _consoleLogService.IsFileLoggingEnabled,
                LogFilePath = _consoleLogService.LogFilePath,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Retorna informações sobre o estado do sistema de log
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetLogStatus()
        {
            return Ok(new
            {
                FileLoggingEnabled = _consoleLogService.IsFileLoggingEnabled,
                LogFilePath = _consoleLogService.LogFilePath,
                LogFileExists = System.IO.File.Exists(_consoleLogService.LogFilePath),
                LogFileSize = System.IO.File.Exists(_consoleLogService.LogFilePath) 
                    ? new System.IO.FileInfo(_consoleLogService.LogFilePath).Length 
                    : 0
            });
        }

        /// <summary>
        /// Simula uma situação com múltiplas mensagens de log
        /// </summary>
        [HttpGet("stress-test")]
        public IActionResult StressTest()
        {
            Console.WriteLine("=== INICIANDO TESTE DE STRESS ===");
            
            for (int i = 1; i <= 10; i++)
            {
                Console.WriteLine($"Mensagem de teste #{i} - {DateTime.Now:HH:mm:ss.fff}");
                Thread.Sleep(100); // Pequeno delay entre mensagens
            }
            
            Console.WriteLine("=== TESTE DE STRESS CONCLUÍDO ===");

            return Ok(new
            {
                Message = "Teste de stress executado - 10 mensagens enviadas para o log",
                Timestamp = DateTime.Now
            });
        }
    }
} 