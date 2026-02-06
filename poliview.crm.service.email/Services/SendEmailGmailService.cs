using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using Poliview.crm.domain;
using Poliview.crm.services;
using Poliview.crm.repositorios;
using ContentType = MimeKit.ContentType;

namespace Poliview.crm.service.email.Services
{
    public class SendEmailGmailService : poliview.crm.service.email.Services.ISendEmailService
    {
        private static string connection = "";
        private static IConfiguration configuration;
        private readonly LogService _logService;
        private readonly INotificacaoErro _notificacaoErro;
        private static bool verQuery = true;
        private static bool verDebug = true;
        private static bool verErros = true;
        private static bool verInfos = true;
        private string _cliente = "não identificado";
        private static string _tituloMensagem = "Envio de Email GMAIL";
        public IEnumerable<Email> EmailsParaEnviar { get; set; }
        public SendEmailGmailService(IConfiguration _configuration, LogService logService, INotificacaoErro notificacaoErro)
        {
            configuration = _configuration;
            _logService = logService;
            _notificacaoErro = notificacaoErro;
            connection = configuration["conexao"].ToString();
            _cliente = configuration["cliente"] ?? "não identificado";
            verQuery = Convert.ToBoolean(configuration["verQuery"]);
            verDebug = Convert.ToBoolean(configuration["verDebug"]);
            verErros = Convert.ToBoolean(configuration["verErros"]);
            verInfos = Convert.ToBoolean(configuration["verInfos"]);
        }

        public async Task EnviarEmailAvulsoAsync(string destinatarios, string assunto, string corpo, ContaEmail conta, Serilog.ILogger log)
        {
            UserCredential credential;
            string ApplicationName = "Poliview CRM";

            // string[] Scopes = { GmailService.Scope.GmailSend, GmailService.Scope.MailGoogleCom, GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify };
            string[] Scopes = { GmailService.Scope.MailGoogleCom };

            try
            {

                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                    Console.WriteLine("Credential file saved to: " + credPath);
                }

                // Cria o serviço da API Gmail
                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var profile = service.Users.GetProfile("me").Execute();

                var nomeRemetente = "";
                var emailRemetente = "";

                string[] emailLista = UtilEmailServices.ExtrairListaEmails(destinatarios);

                var emailPara = new List<MailboxAddress>();
                var emailComCopia = new List<MailboxAddress>();
                var count = 0;

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(nomeRemetente, emailRemetente));

                foreach (var destinatario in emailLista)
                {
                    var e = new MailboxAddress("", destinatario.Trim());

                    if (count == 0)
                    {
                        emailPara.Add(e);
                        emailMessage.To.Add(e);
                    }
                    else
                    {
                        emailComCopia.Add(e);
                        emailMessage.Cc.Add(e);
                    }
                    count++;
                }

                emailMessage.Subject = assunto;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = corpo,
                };

                // bodyBuilder.Attachments.Add(attachmentPath);

                emailMessage.Body = bodyBuilder.ToMessageBody();

                // Solicita confirmação de leitura
                emailMessage.Headers.Add("Disposition-Notification-To", emailRemetente);
                emailMessage.Headers.Add("Return-Receipt-To", emailRemetente);

                var message = new Google.Apis.Gmail.v1.Data.Message
                {
                    Raw = UtilEmailServices.Base64UrlEncodeMimeMessage(emailMessage)
                };

                try
                {

                    var messagePartHeader1 = new MessagePartHeader();
                    messagePartHeader1.Name = "Disposition-Notification-To";

                    var messagePartHeader2 = new MessagePartHeader();
                    messagePartHeader2.Name = "Return-Receipt-To";

                    message.Payload.Headers.Add(messagePartHeader1);
                    message.Payload.Headers.Add(messagePartHeader2);

                    var request = service.Users.Messages.Send(message, "me");
                    var parameters = new Dictionary<string, object>
                    {
                        { "deleteDraft", "true" }
                    };
                    var result = await request.ExecuteAsync();
                    log.Debug($"Enviada mensagem {conta.descricaoConta} - IdMessage: {result.Id}");
                }
                catch (Exception ex)
                {
                    log.Error($"Erro: {ex.Message} - {conta.descricaoConta}");
                    var mensagemErro = $"Erro no envio de email avulso - Conta: {conta.descricaoConta}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                    _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Erro: {ex.Message} - {conta.descricaoConta}");
                var mensagemErro = $"Erro geral no envio de email avulso - Conta: {conta.descricaoConta}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
            }
            finally
            {
                if (verInfos) log.Information($"Processo Finalizado - {conta.descricaoConta}");
            }
            // return true;
        }

        public async void SendEmailAsync(IEnumerable<Email> emailsParaEnvio, ContaEmail conta, Serilog.ILogger log)
        {
            await EnviarEmailGmail(emailsParaEnvio, conta, log);
        }

        private async Task EnviarEmailGmail(IEnumerable<Email> emailsParaEnvio, ContaEmail conta, Serilog.ILogger log)
        {
            var emailService = new EmailService(configuration, log);
            var idatual = 0;

            var chamadoService = new ChamadoService(configuration);

            var i = 1;
            var tot = emailsParaEnvio.Count();

            // string[] Scopes = { GmailService.Scope.GmailSend, GmailService.Scope.MailGoogleCom };

            // UserCredential credential;

            var credential = GoogleCredential.FromJson("{\"client_id\":\"" + conta.client_id + "\",\"client_secret\":\"" + conta.clientSecret + "\",\"refresh_token\":\"" + conta.refreshtoken + "\",\"type\":\"authorized_user\"}")
                                          .CreateScoped(GmailService.Scope.MailGoogleCom);

            /*
			using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
			{
				string credPath = "token.json";
				credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
					GoogleClientSecrets.FromStream(stream).Secrets,
					Scopes,
					"user",
					CancellationToken.None,
					new FileDataStore(credPath, true));
				// Console.WriteLine("Credential file saved to: " + credPath);
			}
            */

            // Cria o servi�o da API Gmail
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Poliview CRM Email Service",
            });


            foreach (var email in emailsParaEnvio)
            {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(email.nomeremetente, email.emailremetente));

                log.Information($"{conta.descricaoConta} - enviando {i} de {tot} idemail={email.id} conta: {conta.descricaoConta} ");
                i += 1;

                idatual = email.id;

                var strassunto = "";
                var dadosenvioemail = $"{conta.descricaoConta} - enviando {i} de {tot} idemail ={email.id}";

                if (email.assunto == "RECUPERAÇÃO DE SENHA")
                {

                    var n1 = email.corpo.IndexOf("<a href=");
                    var n2 = email.corpo.IndexOf("target");
                    var url = email.corpo.Substring(n1 + 8, n2 - n1 - 9);

                    email.corpo = "<p>RECUPERA&Ccedil;&Atilde;O DE SENHA</p> " +
                                    "<p>&nbsp;</p> " +
                                    "<p>Para cadastrar uma nova senha clique <a href='" + url + "' target='_blank' rel='noopener'>aqui</a></p>";
                }
                else
                {
                    email.assunto = UtilEmailServices.RetiraIdEmaildoAssunto(email.assunto);
                    email.assunto = UtilEmailServices.IncluirIdEmailNoAssunto(email.assunto, email.id);

                }

                string[] emailLista;

                emailLista = UtilEmailServices.ExtrairListaEmails(email.emaildestinatario);

                var emailPara = new List<string>();
                var emailComCopia = new List<string>();
                var count = 0;
                var listaDestinatarios = "";
                var assunto = "";

                foreach (var destinatario in emailLista)
                {
                    listaDestinatarios += destinatario + " ";

                    if (string.IsNullOrEmpty(conta.emaildestinatariosuporte))
                    {

                        if (count == 0)
                        {
                            emailPara.Add(destinatario);
                            emailMessage.To.Add(new MailboxAddress("", destinatario));
                        }
                        else
                        {
                            emailComCopia.Add(destinatario);
                            emailMessage.Cc.Add(new MailboxAddress("", destinatario));
                        }
                        count++;
                    }
                    else
                    {
                        if (emailPara.IndexOf(conta.emaildestinatariosuporte.Trim()) == -1)
                        {
                            emailPara.Add(conta.emaildestinatariosuporte.Trim());
                            emailMessage.To.Add(new MailboxAddress("", conta.emaildestinatariosuporte.Trim()));
                        }
                    }

                }

                strassunto = "";

                if (!string.IsNullOrEmpty(conta.emaildestinatariosuporte) && !email.assunto.Contains(" em nome de: "))
                {
                    strassunto = " em nome de: " + listaDestinatarios;
                }

                // email.assunto = email.assunto + strassunto;

                email.assunto = UtilEmailServices.TrocaVariaveisTexto(email.idchamado, email.idocorrencia, email.assunto + strassunto, configuration);
                email.corpo = UtilEmailServices.TrocaVariaveisTexto(email.idchamado, email.idocorrencia, email.corpo, configuration);
                emailService.SalvarCorpoEAssunto(email.id, email.corpo, email.assunto + strassunto, email.idaviso);

                if (!string.IsNullOrEmpty(conta.emaildestinatariosuporte))
                {
                    dadosenvioemail = $"{conta.descricaoConta} - enviar email para {conta.emaildestinatariosuporte} assunto: {email.assunto} ";
                }
                else
                {
                    dadosenvioemail = $"{conta.descricaoConta} - enviar email para {listaDestinatarios} assunto: {email.assunto} ";
                }


                log.Debug(dadosenvioemail);

                var anexos = emailService.ListarAnexosEmail(email.id, log);
                email.erroenvio = "";

                emailMessage.Subject = email.assunto;
                emailMessage.Headers.Add("Disposition-Notification-To", email.emailremetente);
                emailMessage.Headers.Add("Return-Receipt-To", email.emailremetente);

                var contentType = new ContentType("text", "html");
                contentType.Charset = "utf-8"; // Definindo a codificação UTF-8

                // Adiciona o corpo da mensagem como HTML
                var bodyBuilder = new BodyBuilder
                {
                  HtmlBody = email.corpo,
                };



                // Atribui o tipo de conteúdo ao corpo
                //var htmlPart = bodyBuilder.ToMessageBody() as TextPart;
                //if (htmlPart != null)
                //{
                //	htmlPart.ContentType = contentType; // Aplica o ContentType explicitamente
                //}


                if (!string.IsNullOrEmpty(email.urlanexo))
                {
                  if (System.IO.File.Exists(email.urlanexo))
                  {
                    bodyBuilder.Attachments.Add(email.urlanexo);
                  }
                  else
                  {
                    email.erroenvio = $"{conta.descricaoConta} - arquivo {email.urlanexo} não encontrado.";
                  }
                }
                else
                {
                  foreach (var anexo in anexos)
                  {
                    byte[] contentBytes = anexo.arquivo;
                    bodyBuilder.Attachments.Add(anexo.nome + anexo.extensao, contentBytes);
                  }
                }

                emailMessage.Body = bodyBuilder.ToMessageBody();

                try
                {


                  var message = new Google.Apis.Gmail.v1.Data.Message
                  {
                        Raw = UtilEmailServices.Base64UrlEncodeMimeMessage(emailMessage)
                  };

                  var request = service.Users.Messages.Send(message, "me");
                  var parameters = new Dictionary<string, object>
                    {
                      { "deleteDraft", "true" }
                    };

                  var result = await request.ExecuteAsync();

                            // log.Debug($"{conta.descricaoConta} - Enviada mensagem " + message.d.ToString());

                            emailService.MarcarEmailComoEnviado(email.id, log, email.erroenvio, result.Id, "0");
                  log.Debug($"{conta.descricaoConta} - Marcar Email como enviado. Id " + email.id);

                }

                catch (Exception ex)
                {
                    UtilEmailServices.MarcarEmailComErro(email.id, ex.Message, log, connection);
                    log.Error($"{conta.descricaoConta} - Erro: {ex.Message} email: id={email.id} {email.emaildestinatario} {email.assunto}");
                    var mensagemErro = $"Erro no envio de email - Conta: {conta.descricaoConta}\n\n" +
                                     $"Email ID: {email.id}\n\n" +
                                     $"Destinatário: {email.emaildestinatario}\n\n" +
                                     $"Assunto: {email.assunto}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                    _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
                }

            }
        }

        public void Send(string assunto, string corpo, string emailDestinatario)
        {
            throw new NotImplementedException();
        }
    }
}
