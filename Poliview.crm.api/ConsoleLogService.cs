using Microsoft.Extensions.Options;

namespace Poliview.crm.api
{
    /// <summary>
    /// Serviço responsável por configurar o sistema de log personalizado
    /// </summary>
    public class ConsoleLogService : IDisposable
    {
        private readonly CustomLogOptions _options;
        private TextWriter? _originalConsoleOut;
        private FileConsoleWriter? _fileConsoleWriter;
        private bool _isConfigured;

        public ConsoleLogService(IOptions<CustomLogOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Configura o sistema de log para interceptar Console.WriteLine
        /// </summary>
        public void ConfigureConsoleLogging()
        {
            if (_isConfigured)
                return;

            try
            {
                // Salva a referência original do Console.Out
                _originalConsoleOut = Console.Out;

                // Cria o writer personalizado
                _fileConsoleWriter = new FileConsoleWriter(_originalConsoleOut, _options);

                // Substitui o Console.Out pelo writer personalizado
                Console.SetOut(_fileConsoleWriter);

                _isConfigured = true;

                if (_options.FileLoggingEnabled)
                {
                    Console.WriteLine($"Sistema de log personalizado configurado. Logs serão salvos em: {_options.LogFilePath}");
                }
                else
                {
                    Console.WriteLine("Sistema de log personalizado configurado. Log em arquivo desabilitado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao configurar sistema de log personalizado: {ex.Message}");
            }
        }

        /// <summary>
        /// Restaura o Console original
        /// </summary>
        public void RestoreOriginalConsole()
        {
            if (!_isConfigured || _originalConsoleOut == null)
                return;

            try
            {
                Console.SetOut(_originalConsoleOut);
                _fileConsoleWriter?.Dispose();
                _isConfigured = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao restaurar console original: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica se o log em arquivo está habilitado
        /// </summary>
        public bool IsFileLoggingEnabled => _options.FileLoggingEnabled;

        /// <summary>
        /// Obtém o caminho do arquivo de log atual
        /// </summary>
        public string LogFilePath => _options.LogFilePath;

        public void Dispose()
        {
            RestoreOriginalConsole();
        }
    }
} 