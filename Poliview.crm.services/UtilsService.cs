using Dapper;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Poliview.crm.repositorios;
using System.Diagnostics;

namespace Poliview.crm.services
{
    public class UtilsService
    {
        private readonly string _connectionStringMssql;
        private readonly string _connectionStringFb;
        private readonly Microsoft.Data.SqlClient.SqlConnection _connectionMSSQL;
        private readonly FirebirdSql.Data.FirebirdClient.FbConnection _connectionFB;

        public UtilsService(IConfiguration configuration)
        {
            _connectionStringMssql = configuration["conexao"];
            _connectionStringFb = configuration["conexaoFirebird"];
            _connectionMSSQL = new SqlServerConnectionFactory(_connectionStringMssql).CreateConnection();
            _connectionFB = new FirebirdConnectionFactory(_connectionStringFb).CreateConnection();
        }

        public void enviarEmailSuporte(string assunto, string corpo)
        {
            try
            {
                var listaemails = new List<string>();
                listaemails.Add("ronaldo@codemaker.tech");
                // listaemails.Add("ronaldompena@gmail.com");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Suporte", "suporte@codemaker.dev.br"));
                message.Subject = assunto;

                var count = 0;

                foreach (var destinatario in listaemails)
                {
                    var e = new MailboxAddress("", destinatario);

                    if (count == 0)
                    {
                        message.To.Add(e);
                    }
                    else
                    {
                        message.Cc.Add(e);
                    }
                    count++;
                }

                var builder = new BodyBuilder();
                builder.HtmlBody = corpo;

                message.Body = builder.ToMessageBody();

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    Console.WriteLine("smtp.hostinger.com");
                    client.Connect("smtp.hostinger.com", 587, SecureSocketOptions.Auto);
                    client.Authenticate("suporte@codemaker.dev.br", "Master@2024");
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email para suporte: {ex.Message}");
            }
        }

        public void ExcluirRegistrosChaveNula()
        {
            try
            {
                var connection = _connectionFB;
                var sql = $"DELETE FROM CRM_EXCLUSAO WHERE TABELA='CLIENTES' and CHAVE is null";
                connection.ExecuteAsync(sql).Wait();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao excluir registros CRM_EXCLUSAO que estão nulos: {ex.Message}");
            }
        }

        public string RunCommand(string command, string args = "", int timeoutSeconds = 60)
        {
            try
            {
                // Verifica se o executável existe
                if (!File.Exists(command))
                {
                    return $"Erro: Arquivo não encontrado: {command}";
                }

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(command),
                        // Configurações adicionais para melhor compatibilidade
                        StandardOutputEncoding = System.Text.Encoding.UTF8,
                        StandardErrorEncoding = System.Text.Encoding.UTF8
                    };

                    // Usar StringBuilder para capturar output de forma assíncrona
                    var outputBuilder = new System.Text.StringBuilder();
                    var errorBuilder = new System.Text.StringBuilder();

                    process.OutputDataReceived += (sender, e) => {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            outputBuilder.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) => {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            errorBuilder.AppendLine(e.Data);
                        }
                    };

                    Console.WriteLine($"Command: {command} {args}");
                    Console.WriteLine($"Working Directory: {process.StartInfo.WorkingDirectory}");

                    process.Start();

                    // Iniciar leitura assíncrona
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Aguardar com timeout
                    bool finished = process.WaitForExit(timeoutSeconds * 1000);

                    if (!finished)
                    {
                        Console.WriteLine("Processo excedeu o timeout, forçando finalização...");
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                process.WaitForExit(5000); // Aguarda 5 segundos para finalizar
                            }
                        }
                        catch (Exception killEx)
                        {
                            Console.WriteLine($"Erro ao finalizar processo: {killEx.Message}");
                        }
                        return "Erro: Processo excedeu o tempo limite de execução.";
                    }

                    // Aguarda um pouco mais para garantir que toda saída foi capturada
                    Thread.Sleep(100);

                    string output = outputBuilder.ToString().Trim();
                    string error = errorBuilder.ToString().Trim();

                    Console.WriteLine($"Exit Code: {process.ExitCode}");
                    Console.WriteLine("Output: " + output);
                    if (!string.IsNullOrEmpty(error))
                        Console.WriteLine("Erro: " + error);

                    // Verifica códigos de erro específicos
                    if (process.ExitCode == 217)
                    {
                        return "Erro 217: Problema na inicialização/finalização da aplicação externa.";
                    }

                    return process.ExitCode == 0 ? output : $"Error {process.ExitCode}: {error}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);
                return $"Exceção na execução: {ex.Message}";
            }
        }

    }
}
