using System.Text;

namespace Poliview.crm.api
{
    /// <summary>
    /// Writer personalizado que intercepta saídas do Console e escreve em arquivo
    /// </summary>
    public class FileConsoleWriter : TextWriter
    {
        private readonly TextWriter _originalWriter;
        private readonly CustomLogOptions _options;
        private readonly object _lockObject = new object();

        public FileConsoleWriter(TextWriter originalWriter, CustomLogOptions options)
        {
            _originalWriter = originalWriter;
            _options = options;
        }

        public override Encoding Encoding => _originalWriter.Encoding;

        public override void Write(char value)
        {
            _originalWriter.Write(value);
            if (_options.FileLoggingEnabled)
            {
                WriteToFile(value.ToString());
            }
        }

        public override void Write(string? value)
        {
            _originalWriter.Write(value);
            if (_options.FileLoggingEnabled && !string.IsNullOrEmpty(value))
            {
                WriteToFile(value);
            }
        }

        public override void WriteLine()
        {
            _originalWriter.WriteLine();
            if (_options.FileLoggingEnabled)
            {
                WriteToFile(Environment.NewLine);
            }
        }

        public override void WriteLine(string? value)
        {
            _originalWriter.WriteLine(value);
            if (_options.FileLoggingEnabled)
            {
                WriteToFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {value}{Environment.NewLine}");
            }
        }

        public override void WriteLine(object? value)
        {
            _originalWriter.WriteLine(value);
            if (_options.FileLoggingEnabled)
            {
                WriteToFile($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {value}{Environment.NewLine}");
            }
        }

        private void WriteToFile(string content)
        {
            try
            {
                lock (_lockObject)
                {
                    // Garante que o diretório existe
                    var directory = Path.GetDirectoryName(_options.LogFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Verifica se precisa fazer rotação do arquivo
                    CheckAndRotateLogFile();

                    // Escreve no arquivo
                    File.AppendAllText(_options.LogFilePath, content);
                }
            }
            catch (Exception ex)
            {
                // Se falhar ao escrever no arquivo, escreve no console original
                _originalWriter.WriteLine($"Erro ao escrever no arquivo de log: {ex.Message}");
            }
        }

        private void CheckAndRotateLogFile()
        {
            if (!File.Exists(_options.LogFilePath))
                return;

            var fileInfo = new FileInfo(_options.LogFilePath);
            var maxSizeBytes = _options.MaxFileSizeMB * 1024 * 1024;

            if (fileInfo.Length > maxSizeBytes)
            {
                RotateLogFiles();
            }
        }

        private void RotateLogFiles()
        {
            try
            {
                var directory = Path.GetDirectoryName(_options.LogFilePath);
                var fileName = Path.GetFileNameWithoutExtension(_options.LogFilePath);
                var extension = Path.GetExtension(_options.LogFilePath);

                // Move arquivos existentes
                for (int i = _options.KeepLastNFiles - 1; i >= 1; i--)
                {
                    var oldFile = Path.Combine(directory!, $"{fileName}.{i}{extension}");
                    var newFile = Path.Combine(directory!, $"{fileName}.{i + 1}{extension}");

                    if (File.Exists(oldFile))
                    {
                        if (File.Exists(newFile))
                            File.Delete(newFile);
                        File.Move(oldFile, newFile);
                    }
                }

                // Move o arquivo atual para .1
                var currentBackup = Path.Combine(directory!, $"{fileName}.1{extension}");
                if (File.Exists(currentBackup))
                    File.Delete(currentBackup);
                File.Move(_options.LogFilePath, currentBackup);

                // Remove arquivos antigos além do limite
                for (int i = _options.KeepLastNFiles + 1; i <= _options.KeepLastNFiles + 10; i++)
                {
                    var oldFile = Path.Combine(directory!, $"{fileName}.{i}{extension}");
                    if (File.Exists(oldFile))
                        File.Delete(oldFile);
                }
            }
            catch (Exception ex)
            {
                _originalWriter.WriteLine($"Erro ao fazer rotação dos arquivos de log: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _originalWriter?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
} 