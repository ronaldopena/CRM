using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.infra
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    public class EmailDecoder
    {
        /// <summary>
        /// Decodifica texto codificado em Quoted-Printable e remove cabeçalhos de email
        /// </summary>
        /// <param name="encodedText">Texto com caracteres codificados e cabeçalhos</param>
        /// <returns>Texto decodificado limpo com acentuação correta</returns>
        public static string DecodeQuotedPrintable(string encodedText)
        {
            if (string.IsNullOrEmpty(encodedText))
                return encodedText;

            try
            {
                // Remove cabeçalhos de email
                encodedText = RemoveEmailHeaders(encodedText);

                // Remove soft line breaks (=\r\n ou =\n)
                encodedText = encodedText.Replace("=\r\n", "").Replace("=\n", "");

                // Padrão para encontrar sequências Quoted-Printable (=XX)
                var pattern = @"=([0-9A-F]{2})";

                // Cria uma lista de bytes para armazenar os valores decodificados
                var bytes = new System.Collections.Generic.List<byte>();
                int lastIndex = 0;

                var matches = Regex.Matches(encodedText, pattern, RegexOptions.IgnoreCase);

                foreach (Match match in matches)
                {
                    // Adiciona o texto antes do código
                    if (match.Index > lastIndex)
                    {
                        string textBefore = encodedText.Substring(lastIndex, match.Index - lastIndex);
                        bytes.AddRange(Encoding.UTF8.GetBytes(textBefore));
                    }

                    // Converte o código hexadecimal para byte
                    string hexValue = match.Groups[1].Value;
                    byte byteValue = Convert.ToByte(hexValue, 16);
                    bytes.Add(byteValue);

                    lastIndex = match.Index + match.Length;
                }

                // Adiciona o texto restante após o último código
                if (lastIndex < encodedText.Length)
                {
                    string textAfter = encodedText.Substring(lastIndex);
                    bytes.AddRange(Encoding.UTF8.GetBytes(textAfter));
                }

                // Decodifica os bytes usando UTF-8
                string result = Encoding.UTF8.GetString(bytes.ToArray());

                // Remove espaços em branco extras no início
                result = result.TrimStart();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao decodificar: {ex.Message}");
                return encodedText; // Retorna o texto original em caso de erro
            }
        }

        /// <summary>
        /// Remove cabeçalhos de email do texto
        /// </summary>
        private static string RemoveEmailHeaders(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Lista de padrões de cabeçalhos comuns em emails
            var headerPatterns = new[]
            {
            @"Content-Type:\s*text/plain;\s*charset=""?[^""^\n]*""?\s*",
            @"Content-Transfer-Encoding:\s*quoted-printable\s*",
            @"Content-Type:\s*[^\n]*\s*",
            @"Content-Transfer-Encoding:\s*[^\n]*\s*",
            @"MIME-Version:\s*[^\n]*\s*",
            @"Content-Disposition:\s*[^\n]*\s*",
            @"Content-ID:\s*[^\n]*\s*",
            @"Content-Description:\s*[^\n]*\s*"
        };

            // Remove cada tipo de cabeçalho
            foreach (var pattern in headerPatterns)
            {
                text = Regex.Replace(text, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            // Remove linhas vazias consecutivas no início
            text = Regex.Replace(text, @"^\s*[\r\n]+", "", RegexOptions.Multiline);

            return text;
        }

        /// <summary>
        /// Corrige automaticamente textos com problemas de acentuação e remove cabeçalhos
        /// Detecta e corrige Quoted-Printable automaticamente
        /// </summary>
        public static string FixEncoding(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Remove cabeçalhos primeiro
            text = RemoveEmailHeaders(text);

            // Verifica se o texto contém padrões Quoted-Printable
            if (Regex.IsMatch(text, @"=[0-9A-F]{2}", RegexOptions.IgnoreCase))
            {
                return DecodeQuotedPrintable(text);
            }

            return text.TrimStart();
        }

        /// <summary>
        /// Versão completa que limpa todo o texto de email
        /// Remove cabeçalhos, decodifica e limpa espaços extras
        /// </summary>
        public static string CleanEmailText(string emailText)
        {
            if (string.IsNullOrEmpty(emailText))
                return emailText;

            // Remove cabeçalhos
            emailText = RemoveEmailHeaders(emailText);

            // Decodifica quoted-printable
            emailText = DecodeQuotedPrintable(emailText);

            // Remove múltiplas linhas vazias (mantém apenas uma)
            emailText = Regex.Replace(emailText, @"(\r?\n\s*){3,}", "\n\n");

            // Remove espaços no final das linhas
            emailText = Regex.Replace(emailText, @"[ \t]+$", "", RegexOptions.Multiline);

            // Trim geral
            emailText = emailText.Trim();

            return emailText;
        }
    }

}
