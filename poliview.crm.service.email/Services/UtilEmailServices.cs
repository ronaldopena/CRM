using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Data.SqlClient;
using MimeKit;
using Dapper;
using Poliview.crm.services;
using System.Collections;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Poliview.crm.service.email.Services
{
    public static class UtilEmailServices
    {        
        public static string TrocaVariaveisTexto(int chamado, int ocorrencia, string texto, IConfiguration configuration)
        {
            ocorrencia = ocorrencia > 0 ? ocorrencia : 1;

            var chamadoservice = new ChamadoService(configuration);
            var dadosChamado = chamadoservice.ListarChamadoDetalhe(chamado, ocorrencia);

            if (chamado > 0)
            {                
                texto = texto.Replace("[CHAMADO]", dadosChamado.idchamado.ToString());
                texto = texto.Replace("[OCORRENCIA]", dadosChamado.idocorrencia.ToString());
                texto = texto.Replace("[NOME_CLIENTE]", dadosChamado.nomecliente);
                texto = texto.Replace("[CLIENTE]", dadosChamado.nomecliente);
                texto = texto.Replace("[EMAIL_CLIENTE]", dadosChamado.emailcliente);
                texto = texto.Replace("[TELEFONE_CLIENTE]", dadosChamado.telefone);
                texto = texto.Replace("[CELULAR_CLIENTE]", dadosChamado.celular);
                texto = texto.Replace("[EMPREENDIMENTO]", dadosChamado.empreendimento);
                texto = texto.Replace("[BLOCO]", dadosChamado.bloco);
                texto = texto.Replace("[UNIDADE]", dadosChamado.unidade);
                texto = texto.Replace("[TIPO_OCORRENCIA]", dadosChamado.tipoocorrencia);
                texto = texto.Replace("[DESCRICAO_CHAMADO]", dadosChamado.descricao);
                texto = texto.Replace("[STATUS_CHAMADO]", dadosChamado.statusChamado);
                texto = texto.Replace("[CHAMADO_STATUS]", dadosChamado.statusChamado);                
                texto = texto.Replace("[STATUS_OCORRENCIA]", dadosChamado.statusOcorrencia);
                texto = texto.Replace("[OCORRENCIA_STATUS]", dadosChamado.statusOcorrencia);
                texto = texto.Replace("[NOME_EMPRESA]", dadosChamado.nomeempresa);
                texto = texto.Replace("[LOGO_EMPRESA]", dadosChamado.logoempresa);
            }

            return texto;
        }                

        public static bool AssuntoContemNumeroChamado(string assunto)
        {
            string pattern = @"<<([^}]+)>>";
            Match m = Regex.Match(assunto, pattern);
            return m.Success;
        }

        public static bool AssuntoContemIdEmail(string assunto)
        {
            string pattern = @"<#([^}]+)#>";
            Match m = Regex.Match(assunto, pattern);
            return m.Success;
        }

        public static int retornaIdEmail(string assunto)
        {
            string pattern = @"<#([^}]+)#>";
            Match m = Regex.Match(assunto, pattern);            
            return Convert.ToInt32(m.Groups[1]);
        }

        public static string IncluirNumeroChamadoNoAssunto(string assunto, string chamado, string ocorrencia)
        {
            var ret = assunto;

            if (!AssuntoContemNumeroChamado(assunto))
            {
                if (string.IsNullOrEmpty(ocorrencia))
                {
                    ret += String.Format(" <<{0}|1>>", chamado);
                }
                else
                {
                    ret += String.Format(" <<{0}|{1}>>", chamado, ocorrencia);
                }
            }

            return ret;
        }

        public static string IncluirIdEmailNoAssunto(string assunto, int idemail)
        {   
            var ret = assunto;
            if (!AssuntoContemIdEmail(assunto))
            {
                ret += $" <#{idemail}#>";
            }
            return ret;
        }

        public static string? RetiraIdEmaildoAssunto(string assunto)
        {            
            string pattern = @"<#([^}]+)#>";
            Match m = Regex.Match(assunto, pattern);
            if (m.Success) 
                return assunto?.Replace(m.Value, "");
            else
                return assunto;
        }

        public static int retornaIdChamado(string assunto)
        {
            var dados = retornaIdChamadoOcorrencia(assunto);
            return Convert.ToInt32(dados[0]?.ToString()); 
        }

        public static ArrayList retornaIdChamadoOcorrencia(string assunto)
        {
            var ret = new ArrayList();

            try
            {
                // assunto = assunto.Substring(0, assunto.IndexOf(">>") + 2);
                string pattern = @"<<([^}]+)>>";
                Match m = Regex.Match(assunto, pattern);

                if (m.Success)
                {
                    string[] cd = m.Groups[1].ToString().Split('|');
                    if (cd.Length == 2)
                    {
                        ret.Add(Convert.ToInt32(cd[0]));
                        if (Convert.ToInt32(cd[1]) < 0)
                            ret.Add(1);
                        else
                            ret.Add(Convert.ToInt32(cd[1]));
                    }
                    else
                    {
                        ret.Add(Convert.ToInt32(cd[0]));
                        ret.Add(1);
                    }
                }
                else
                {
                    ret.Add(0);
                    ret.Add(0);
                }
            }
            catch (Exception)
            {
                ret.Add(0);
                ret.Add(0);

            }
            return ret;
        }

        public static bool ExisteTabelaDetalheChamado(string texto)
        {
            return texto.Contains("##TABDETCHA##");
        }

        public static string RetornaTabelaDetalhesChamado(IConfiguration configuration, int idchamado, int idocorrencia = 1)
        {
            var emailCorpo = "";

            idocorrencia = idocorrencia > 0 ? idocorrencia : 1;

            var chamadoService = new ChamadoService(configuration);
            var chamado = chamadoService.ListarChamadoDetalhe(idchamado, idocorrencia);

            if (chamado.idchamado > 0)
            {
                emailCorpo += "<!-- ##TABDETCHA## --> ";
                emailCorpo += "<p>Informações do Chamado:</p>";
                emailCorpo += "<table border='1' bordercolor='#000000' cellspacing='0' cellpadding='3' style='border-collapse: collapse; width: 100%;'>";
                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left' style='width: 25%'>Chamado Nº</td>";
                emailCorpo += String.Format("<td align='Left' style='width: 75%'>{0}</td>", chamado.idchamado);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Ocorrência</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.idocorrencia);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Cliente</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.nomecliente);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Email</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.emailcliente);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Telefone</td>";
                emailCorpo += String.Format("<td align='Left'>{0} {1}</td>", chamado.ddd, chamado.telefone);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Celular</td>";
                emailCorpo += String.Format("<td align='Left'>{0} {1}</td>", chamado.ddd, chamado.celular);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Empreendimento</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.empreendimento);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Bloco</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.bloco);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Unidade</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.unidade);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left'>Tipo Ocorrência</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.tipoocorrencia);
                emailCorpo += "</tr>";

                emailCorpo += "<tr>";
                emailCorpo += "<td align='Left' valign='Top'>Descrição do Chamado</td>";
                emailCorpo += String.Format("<td align='Left'>{0}</td>", chamado.descricao);
                emailCorpo += "</tr>";
            }
            return emailCorpo;
        }

        private static string HtmlToPlainText(string html)
        {
            const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";//matches one or more (white space or line breaks) between '>' and '<'
            const string stripFormatting = @"<[^>]*(>|$)";//match any character between '<' and '>', even when end tag is missing
            const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";//matches: <br>,<br/>,<br />,<BR>,<BR/>,<BR />
            var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
            var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
            var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

            var text = html;
            //Decode html specific characters
            text = WebUtility.HtmlDecode(text);
            //Remove tag whitespace/line breaks
            text = tagWhiteSpaceRegex.Replace(text, "><");
            //Replace <br /> with line breaks
            text = lineBreakRegex.Replace(text, Environment.NewLine);
            //Strip formatting
            text = stripFormattingRegex.Replace(text, string.Empty);

            text = Regex.Replace(text, "<style>(.|\n)*?<style>", String.Empty);
            text = Regex.Replace(text, @"<[^\>]*>", String.Empty);

            text = text.Replace(@"v\:* {behavior:url(#default#VML);}", String.Empty);
            text = text.Replace(@"o\:* {behavior:url(#default#VML);}", String.Empty);
            text = text.Replace(@"w\:* {behavior:url(#default#VML);}", String.Empty);
            text = text.Replace(@".shape {behavior:url(#default#VML);}", String.Empty);
            return text;
        }

        public static string? ExtrairEmail(string texto)
        {
            string pattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b";                              
            Match m = Regex.Match(texto, pattern);
            return m.Value;
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

        public static string CorpoEmailRecuperacaoSenha(string emailCorpo)
        {
            var n1 = emailCorpo.IndexOf("<a href=");
            var n2 = emailCorpo.IndexOf("target");
            var url = emailCorpo.Substring(n1 + 8, n2 - n1 - 9);

            emailCorpo = "<p>RECUPERA&Ccedil;&Atilde;O DE SENHA</p> " +
                            "<p>&nbsp;</p> " +
                            "<p>Para cadastrar uma nova senha clique <a href='" + url + "' target='_blank' rel='noopener'>aqui</a></p>";

            return emailCorpo;

        }

        public static string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        public static string Base64UrlEncodeMimeMessage(MimeMessage mimeMessage)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                mimeMessage.WriteTo(memoryStream);
                var bytes = memoryStream.ToArray();
                return Convert.ToBase64String(bytes)
                              .Replace("+", "-")
                              .Replace("/", "_")
                              .Replace("=", "");
            }
        }

        public static string Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 0: break;
                case 2: s += "=="; break;
                case 3: s += "="; break;
                default: throw new System.Exception("Illegal base64url string!");
            }
            var bytes = Convert.FromBase64String(s);
            return System.Text.Encoding.UTF8.GetString(bytes);
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

        public static void MarcarEmailComErro(int idemail, string mensagemErro, Serilog.ILogger log, string _connectionString)
        {
            using var connection = new SqlConnection(_connectionString);
            // var query = "UPDATE OPE_EMAIL SET IN_EmailStatusEnvio = 9, DS_ErroEnvio = @mensagemErro, QTD_TentativasEnvio = COALESCE(QTD_TentativasEnvio, 0) + 1 WHERE id_email = @idemail";
            var query = "exec CRM_Marca_Email_Com_Erro @id_email=@IdEmail, @mensagem_erro=@MensagemErro";
            var parameters = new
            {
                MensagemErro = mensagemErro,
                Idemail = idemail
            };
            connection.Open();
            connection.Execute(query, parameters);
            connection.Close();
        }


    }





}

