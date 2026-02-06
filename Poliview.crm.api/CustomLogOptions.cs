namespace Poliview.crm.api
{
    /// <summary>
    /// Configurações para o sistema de log personalizado
    /// </summary>
    public class CustomLogOptions
    {
        /// <summary>
        /// Define se o log em arquivo está habilitado
        /// </summary>
        public bool FileLoggingEnabled { get; set; } = false;

        /// <summary>
        /// Caminho do arquivo de log
        /// </summary>
        public string LogFilePath { get; set; } = "logs/console-output.log";

        /// <summary>
        /// Tamanho máximo do arquivo em MB antes de fazer rotação
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 10;

        /// <summary>
        /// Quantidade de arquivos de log antigos para manter
        /// </summary>
        public int KeepLastNFiles { get; set; } = 5;
    }
} 