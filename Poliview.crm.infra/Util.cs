using Poliview.crm.domain;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Poliview.crm.infra
{
    public class Util
    {
        public static string RunCommand(string command, string args)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrEmpty(error)) { return output; }
            else { return error; }
        }

        public static string Decodificar(string str, Boolean criptografar = false)
        {
            string decodedString = str;

            try
            {
                if (!string.IsNullOrEmpty(str) && criptografar)
                {
                    byte[] data = Convert.FromBase64String(str);
                    decodedString = Encoding.UTF8.GetString(data);
                }

            }
            catch (Exception)
            {

            }

            return decodedString;
        }

        public static string Codificar(string str, Boolean criptografar = false)
        {
            string encodedString = str;

            try
            {
                if (!string.IsNullOrEmpty(str) && criptografar)
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
                    encodedString = Convert.ToBase64String(data);
                }

            }
            catch (Exception)
            {

            }

            return encodedString;
        }

        public static Boolean ExcluirArquivosPDF(string pathInstalacao)
        {
            var pathArquivos = pathInstalacao;

            if (Directory.Exists(pathArquivos))
            {
                string[] arquivos = Directory.GetFiles(pathArquivos, "*.*");

                if (arquivos.Length > 0)
                {
                    // EventLog.WriteEntry(EmailBL.RetornaNomeInstancia("PoliviewCrmMonitor"), "excluindo " + arquivos.Length.ToString() + "arquivos temporários em " + pathArquivos, EventLogEntryType.Information);
                }

                foreach (string arq in arquivos)
                {

                    FileInfo infoArquivo = new FileInfo(arq);

                    if (infoArquivo.CreationTime < DateTime.Now.AddHours(-1))
                    {
                        try
                        {
                            infoArquivo.Delete();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao excluir o arquivo: {infoArquivo.Name} - ${ex.Message}");
                        }
                    }

                }

            }
            else
            {
                // EventLog.WriteEntry(EmailBL.RetornaNomeInstancia("PoliviewCrmMonitor"), "Caminho não encontrado " + pathArquivos, EventLogEntryType.Error);
            }

            return true;

        }

        public static bool emailValido(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        public static string RemoverPontosTracosBarras(string input)
        {
            // Utilize a expressão regular [.\-/] para corresponder a pontos, traços e barras
            // e substituí-los por uma string vazia
            string pattern = "[.\\-/]";
            string resultado = Regex.Replace(input, pattern, "");

            return resultado;
        }

        public static string urlApiCrm(HttpClient http)
        {
            var url = http.BaseAddress?.ToString();
            if (url != null && url.EndsWith("/"))
            {
                return url.TrimEnd('/');
            }
            return url ?? string.Empty;
        }

        public static void ExibirPropriedades<T>(T objeto)
        {
            if (objeto == null)
            {
                Console.WriteLine("O objeto é nulo.");
                return;
            }

            Type tipo = objeto.GetType();
            PropertyInfo[] propriedades = tipo.GetProperties();

            Console.WriteLine($"Propriedades do objeto {tipo.Name}:");

            foreach (PropertyInfo propriedade in propriedades)
            {
                object? valor = propriedade.GetValue(objeto);
                Console.WriteLine($"{propriedade.Name}: {valor}");
            }
        }

        public static string ExibirPropriedadesNotificacao<T>(T objeto)
        {
            var ret = new StringBuilder();
            if (objeto == null)
            {
                return "O objeto é nulo.";
            }

            Type tipo = objeto.GetType();
            PropertyInfo[] propriedades = tipo.GetProperties();

            ret.Append($"Propriedades do objeto {tipo.Name}: \n\n");

            foreach (PropertyInfo propriedade in propriedades)
            {
                object? valor = propriedade.GetValue(objeto);
                ret.Append($"{propriedade.Name}: {valor} \n");
            }

            return ret.ToString();
        }

        public static string[] ExtrairListaEmails(string texto)
        {
            if (texto == null)
                return new String[0];
            
            // Define a regex pattern to match emails
            string pattern = @"([a-zA-Z0-9._-]+@[a-zA-Z0-9._-]+\.[a-zA-Z0-9._-]+)";

            // Create a new regex object
            Regex regex = new Regex(pattern);

            // Find all matches in the text
            MatchCollection matches = regex.Matches(texto);

            // Create a new string array to store the emails
            string[] emails = new string[matches.Count];

            // Loop through the matches and add each email to the array
            for (int i = 0; i < matches.Count; i++)
            {
                emails[i] = matches[i].Value;
            }

            // Return the array of emails
            return emails;

        }

        public static string LimparListaEmails(string email)
        {
            var retorno = "";
            if (email != null)
            {
                var emails = ExtrairListaEmails(email);
                foreach (var e in emails)
                {
                    if (retorno != "") retorno += ";";
                    retorno += e.Trim().ToString();
                }
            }
            return retorno;
        }

    }
}
