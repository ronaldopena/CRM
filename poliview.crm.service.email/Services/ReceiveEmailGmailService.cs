using Poliview.crm.domain;
using Poliview.crm.service.email.Services;
using poliview.crm.service.email.Services;
using Poliview.crm.services;
using Poliview.crm.repositorios;
using Google.Apis.Gmail.v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Gmail.v1.Data;
using MimeKit;

namespace Poliview.crm.service.email.Services
{
    public class ReceiveEmailGmailService : IReceiveEmailService
    {
        private static string connection = "";
        private static IConfiguration _configuration;
        private readonly LogService _logService;
        private readonly INotificacaoErro _notificacaoErro;
        private static bool verQuery = true;
        private static bool verDebug = true;
        private static bool verErros = true;
        private static bool verInfos = true;
        private string _cliente = "não identificado";
        private static string _tituloMensagem = "Recebimento de Email Gmail";

        public ReceiveEmailGmailService(IConfiguration configuration, LogService logService, INotificacaoErro notificacaoErro)
        {
            _configuration = configuration;
            _logService = logService;
            _notificacaoErro = notificacaoErro;
            connection = configuration["conexao"].ToString();
            _cliente = configuration["cliente"] ?? "não identificado";
            verQuery = Convert.ToBoolean(configuration["verQuery"]);
            verDebug = Convert.ToBoolean(configuration["verDebug"]);
            verErros = Convert.ToBoolean(configuration["verErros"]);
            verInfos = Convert.ToBoolean(configuration["verInfos"]);
        }

        public async Task<bool> ReceiveEmailAsync(Serilog.ILogger log, ContaEmail conta)
        {
            await ReceberEmailGmail(log, conta);
            ProcessarEmailsRecebidos.Processar(log, _configuration, verInfos);
            return true;
        }

        private async Task ReceberEmailGmail(Serilog.ILogger log, ContaEmail conta)
        {
            var dadosDoEmail = "";
            var msg = "";
            var EmailDoCrm = "";

            try
            {
                log.Information($"{conta.descricaoConta} - RECEBIMENTO DE EMAIL GMAIL INICIADO");

                string[] Scopes = { GmailService.Scope.MailGoogleCom };
                string ApplicationName = "Poliview CRM";

                UserCredential credential;

                Console.WriteLine("Credentials");
                //using (var stream = new FileStream("client_secret_775433365105-7b69qo8knaiukqqrpgpj00urvmup4l5e.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
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

                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                Console.WriteLine("Inicializando GmailService");

                var profile = service.Users.GetProfile("me").Execute();
                var id = "";
                var email = new Email();
                var emailService = new EmailService(_configuration, log);

                var arquivoService = new ArquivoService(_configuration);
                var idemailentrada = 0;

                dadosDoEmail = "";
                var idemail = 0;
                var idaviso = 0;
                var listaAnexos = new List<int>();
                var i = 0;
                var emailDoDestinatario = "";
                var TamanhoMaximoAnexosAtingido = false;

                //var result = await ListMessages(service, "me");
                //var mensagens = result.Messages;


                var request = service.Users.Messages.List("me");
                request.LabelIds = "INBOX";  // Define para ler mensagens da Caixa de Entrada
                request.IncludeSpamTrash = false;  // Não inclui spam ou lixeira
                Console.WriteLine("Lista de mensagens");

                var response = await request.ExecuteAsync();

                Console.WriteLine("Request Lista de mensagens");

                var mensagens = response.Messages;

                if (mensagens == null) return;

                if (verInfos)
                {
                    log.Information($"{conta.descricaoConta} - Foram encontradas {mensagens.Count} mensagens na caixa de entrada!");
                    log.Information("");
                }

                foreach (var mensagem in mensagens)
                {
                    Console.WriteLine("Checando se a mensagem é nula");
                    if (mensagem == null) continue;

                    var emailDetails = await service.Users.Messages.Get("me", mensagem.Id).ExecuteAsync();
                    Console.WriteLine("detalhe da mensagem");

                    string remetente = "";
                    string destinatario = "";
                    string assunto = "";
                    string corpo = "";

                    var corpohtml = "";
                    var corpotext = "";
                    var corpooutros = "";

                    var TamanhoTotalAnexos = 0;
                    List<string> anexos = new List<string>();

                    // Extrai o remetente, destinatário, assunto e corpo da mensagem
                    foreach (var header in emailDetails.Payload.Headers)
                    {
                        if (header.Name == "From")
                            remetente = UtilEmailServices.ExtrairEmail(header.Value);
                        if (header.Name == "To")
                            destinatario = header.Value;
                        if (header.Name == "Subject")
                            assunto = header.Value;
                    }
                    Console.WriteLine("Leu o header");

                    if (remetente == null)
                    {
                        await DeletarEmailCaixaEntradaAsync(service, "me", mensagem.Id, log);
                        continue;
                    }

                    // Processa as partes do corpo da mensagem (MIME structure)
                    var messagePart = emailDetails.Payload.Parts;
                    var snippet = "";

                    Console.WriteLine("Chamando o GetBody");
                    corpo = GetBody(emailDetails);

                    corpo = Poliview.crm.infra.EmailDecoder.FixEncoding(corpo);

                    // mensagem.Payload.Body.Data

                    if (messagePart != null)
                    {
                        snippet = mensagem.Snippet;

                        foreach (var part in messagePart)
                        {
                            /*
                            if (part.Body == null)
                            {
								foreach (var p in part.Parts)
								{
									if (p.MimeType == "text/plain") corpotext = GetDecodedBody(p.Body.Data);
									else if (p.MimeType == "text/html") corpohtml = GetDecodedBody(p.Body.Data);
									else corpooutros = GetDecodedBody(p.Body.Data);
								}
							}
                            else
                            {
								if (part.MimeType == "text/plain") corpotext = GetDecodedBody(part.Body.Data);
								else if (part.MimeType == "text/html") corpohtml = GetDecodedBody(part.Body.Data);
								else corpooutros = GetDecodedBody(part.Body.Data);
							}
                            */
                            if (!string.IsNullOrEmpty(part.Filename))
                            {
                                var attachmentId = part.Body?.AttachmentId;

                                if (!string.IsNullOrEmpty(attachmentId))
                                {
                                    var attachmentSize = part.Body?.Size;
                                    TamanhoTotalAnexos += (int)attachmentSize;
                                    anexos.Add($"{part.Filename} - {attachmentSize}");
                                }
                            }
                        }
                    }

                    if (TamanhoTotalAnexos > conta.tamanhomaximoanexos * 1024 * 1024)
                    {
                        emailService.EnviarEmailTamanhoMaximoAnexoAtingido(email, log);
                        await DeletarEmailCaixaEntradaAsync(service, conta.userId, mensagem.Id, log);
                        continue;
                    }

                    /*
                    if (!string.IsNullOrEmpty(corpo)) corpo = corpo;
                    else if (!string.IsNullOrEmpty(corpohtml)) corpo = corpohtml;
                    else if (!string.IsNullOrEmpty(corpotext)) corpo = corpotext;
                    */

                    i += 1;
                    log.Information($"{conta.descricaoConta} - RECEBENDO MENSAGEM {i} de {mensagens.Count} de {conta.descricaoConta} ");
                    msg = "";

                    var ids = UtilEmailServices.retornaIdChamadoOcorrencia(assunto);
                    email.nomeremetente = "";
                    //email.emailremetente = mensagem.From.EmailAddress.Address.Trim();
                    var remetenteEmail = UtilEmailServices.ExtrairListaEmails(remetente);
                    email.emailremetente = remetenteEmail[0].Trim().ToString();
                    email.emaildestinatario = destinatario;
                    // email.emaildestinatario = mensagem.ToRecipients.FirstOrDefault().EmailAddress.Address;
                    email.datainclusao = DateTime.Now;
                    email.assunto = assunto;
                    email.corpo = corpo;
                    email.corpohtml = 1;
                    email.idstatusenvio = 0;
                    email.dataenvio = DateTime.MinValue;
                    email.tipoemail = "R";
                    email.erroenvio = "";
                    email.idchamado = Convert.ToInt16(ids[0].ToString());
                    email.idocorrencia = Convert.ToInt16(ids[1].ToString());
                    email.entregue = 0;
                    email.processado = 0;
                    email.urlanexo = "";
                    email.idcontaemail = conta.id;
                    email.classificacaoemail = 7; // email recebido

                    emailService.SalvarQuarentena(email, log, ref msg);

                    try
                    {
                        if (id == "") id = mensagem.Id;

                        if (destinatario != null) // && me.Mail.ToLower().Trim() != mensagem.From.EmailAddress.Address.ToLower().Trim()
                        {
                            msg = "";

                            if (email.idocorrencia == 0) email.idocorrencia = 1;

                            dadosDoEmail = $"{conta.descricaoConta} - id: {email.id} | remetente: {email.emailremetente} | destinatário: {email.emaildestinatario} | Assunto: {email.assunto}";

                            idemail = emailService.RetornaIdEmail(email);

                            var confirmacaoentrega = emailService.EmailDeConfirmacaoEntrega(email);
                            var idemailnovo = 0;

                            if (confirmacaoentrega > 0)
                            {
                                await DeletarEmailCaixaEntradaAsync(service, "me", mensagem.Id, log);

                                if (confirmacaoentrega == 1)
                                    emailService.MarcarEmailComoConfirmadoProvedor(email, log);
                                else
                                    emailService.MarcarEmailComoRecusadoProvedor(email, log);

                                if (verInfos) log.Information($"{conta.descricaoConta} - CONFIRMAÇÃO EMAIL {dadosDoEmail}");
                            }
                            else
                            {
                                if (emailService.EmailCrm(email) && email.emailremetente != email.emaildestinatario)
                                {
                                    

                                    var chamadoConcluido = emailService.VerificaEmailParaChamadoConcluido(email, 0, log);
                                    var chamadoCancelado = emailService.VerificaEmailParaChamadoCancelado(email, 0, log);
                                    idaviso = emailService.SalvarAviso(email, ref msg, log);
                                    email.idaviso = idaviso;

                                    emailService.SalvarEmailNoHistorico(email, ref msg, log);

                                    dadosDoEmail = $"{conta.descricaoConta} -  id: {email.id} | remetente: {email.emailremetente} | destinatário: {email.emaildestinatario} | Assunto: {email.assunto}";
                                    if (chamadoConcluido)
                                        dadosDoEmail += " EMAIL PARA CHAMADO CONCLUIDO";

                                    if (chamadoCancelado)
                                        dadosDoEmail += " EMAIL PARA CHAMADO CANCELADO";

                                    if (verDebug) log.Debug($"Email Recebido: {dadosDoEmail}");
                                    idemailnovo = emailService.Salvar(email, log, ref msg);
                                    log.Debug($"Id Novo Email: {idemailnovo}");
                                    email.id = idemailnovo;
                                    idemailentrada = idemailnovo;


                                    if ((!chamadoConcluido) && (!chamadoCancelado))
                                    {
                                        var idanexo = 0;
                                        listaAnexos = new List<int>();


                                        if (messagePart != null)
                                        {
                                            // messagePart é nulo
                                            foreach (var part in messagePart)
                                            {
                                                if (!string.IsNullOrEmpty(part.Filename))
                                                {
                                                    var attachmentId = part.Body?.AttachmentId;

                                                    if (!string.IsNullOrEmpty(attachmentId))
                                                    {
                                                        var attachment = service.Users.Messages.Attachments.Get("me", mensagem.Id, attachmentId).Execute();

                                                        if (attachment != null)
                                                        {
                                                            // UtilServices.Base64UrlDecode
                                                            // byte[] attachmentData = Convert.FromBase64String(attachment.Data)

                                                            byte[] attachmentData = Convert.FromBase64String(Base64UrlToBase64(attachment.Data));

                                                            idanexo = 0;
                                                            var arquivo = new Arquivo();

                                                            var nomeArq = Path.GetFileNameWithoutExtension(part.Filename);
                                                            var extArq = Path.GetExtension(part.Filename);

                                                            if (nomeArq.Length + extArq.Length > 100)
                                                                nomeArq = nomeArq.Substring(0, 99 - extArq.Length);

                                                            arquivo.arquivo = nomeArq;
                                                            arquivo.extensao = extArq;
                                                            arquivo.conteudo = attachmentData;
                                                            arquivo.banco = "0";
                                                            arquivo.padrao = "N";
                                                            arquivo.chamado = "S";
                                                            arquivo.idusuario = 1;
                                                            arquivo.data = DateTime.Now;
                                                            if (!part.Filename.ToLower().Contains("image001") && !part.Filename.ToLower().Contains("image002"))
                                                            {
                                                                idanexo = arquivoService.Salvar(arquivo, log, ref msg);
                                                                if (idanexo != 0)
                                                                {
                                                                    listaAnexos.Add(idanexo);
                                                                    if (email.idchamado > 0)
                                                                        arquivoService.SalvarAnexoChamado(email.idchamado, email.idocorrencia, idanexo, arquivo.arquivo + arquivo.extensao, log, ref msg);
                                                                }

                                                            }
                                                            if (idaviso > 0 && idanexo > 0 && email.id > 0)
                                                                emailService.SalvarAvisoEmail(email.id, idanexo, idaviso);
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    idemailnovo = emailService.SalvarQuarentena(email, log, ref msg);
                                    log.Debug($"Não é email do CRM: {idemailnovo} ");
                                }
                                await DeletarEmailCaixaEntradaAsync(service, "me", mensagem.Id, log);
                            }
                        }
                        else
                        {
                            if (verInfos) log.Information($"{conta.descricaoConta} - Mensagem Excluída! {mensagem.Id}");
                            await DeletarEmailCaixaEntradaAsync(service, "me", mensagem.Id, log);
                        }

                    }
                    catch (Exception ex)
                    {
                        foreach (var idanexoErro in listaAnexos)
                        {
                            emailService.ExcluirAnexo(idanexoErro, log);
                        }

                        if (idaviso > 0)
                            emailService.ExcluirAviso(idaviso, log);

                        if (idemailentrada > 0)
                        {
                            emailService.ExcluirEmail(idemailentrada, log);
                            log.Error($"{conta.descricaoConta} - ERRO AO RECEBER EMAIL ReceiveEmailService.cs(451)" + ex.Message + " " + dadosDoEmail);
                        }

                        var mensagemErro = $"Erro no recebimento de email do cliente: {_cliente ?? "Não identificado"}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                        _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);

                        log.Error($"{conta.descricaoConta} - ERRO AO RECEBER EMAIL MSG " + msg);
                    }

                }
                if (verInfos) log.Information($"{conta.descricaoConta} - RECEBIMENTO DE EMAIL FINALIZADO " + conta.descricaoConta);
            }
            catch (Exception ex)
            {
                log.Error($"{conta.descricaoConta} - ERRO DE CONEXÃO COM O GMAIL - {ex.Message} - {dadosDoEmail}");
                var mensagemErro = $"Erro ao receber email Gmail - Conta: {conta.descricaoConta}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
            }
        }

        static MimeMessage CreateEmailWithAttachment(string to, string from, string subject, string body, string attachmentPath)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("", from));
            emailMessage.To.Add(new MailboxAddress("", to));
            emailMessage.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                TextBody = body,
            };

            bodyBuilder.Attachments.Add(attachmentPath);

            emailMessage.Body = bodyBuilder.ToMessageBody();

            // Solicita confirmação de leitura
            emailMessage.Headers.Add("Disposition-Notification-To", from);
            emailMessage.Headers.Add("Return-Receipt-To", from);

            return emailMessage;
        }

        static async Task<Message?> SendMessage(GmailService service, string userId, MimeMessage emailContent)
        {

            var message = new Message
            {
                Raw = UtilEmailServices.Base64UrlEncode(emailContent.ToString())
            };

            try
            {
                var request = service.Users.Messages.Send(message, userId);
                var parameters = new Dictionary<string, object>
                {
                    { "deleteDraft", "true" }
                };
                // request.RequestParameters
                //    Google.Apis.Discovery.IParameter
                //request.RequestParameters.Add("deleteDraft", Google.Apis.Discovery.Parameter.); // Para evitar que o email apareça nos itens enviados
                // var result = await service.Users.Messages.Send(message, userId).ExecuteAsync();
                var result = await request.ExecuteAsync();

                Console.WriteLine("Mensagem enviada com Id: " + result.Id);
                return await request.ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ocorreu um erro: " + e.Message);
                return null;
            }


        }

        static async Task<ListMessagesResponse?> ListMessages(GmailService service, string userId)
        {
            try
            {
                return await service.Users.Messages.List(userId).ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ocorreu um erro: " + e.Message);
                return null;
            }
        }

        static async Task<Message> GetMessage(GmailService service, string userId, string messageId)
        {
            try
            {
                return await service.Users.Messages.Get(userId, messageId).ExecuteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Ocorreu um erro: " + e.Message);
                return null;
            }
        }

        private async Task DeletarEmailCaixaEntradaAsync(GmailService service, string UserId, string MessageId, Serilog.ILogger log)
        {
            try
            {
                await service.Users.Messages.Delete(UserId, MessageId).ExecuteAsync();
                log.Information("mensagem excluida com sucesso!");
            }
            catch (Exception ex)
            {
                var mensagemErro = $"Erro ao deletar email na caixa de entrada - Conta: {UserId}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
                log.Error(ex.Message);
            }
        }

        public static string GetSender(Message message)
        {
            if (message == null || message.Payload == null || message.Payload.Headers == null)
                return null;

            foreach (var header in message.Payload.Headers)
            {
                if (header.Name == "From")
                {
                    return header.Value;
                }
            }

            return null;
        }

        public static string GetSubject(Message message)
        {
            if (message == null || message.Payload == null || message.Payload.Headers == null)
                return null;

            foreach (var header in message.Payload.Headers)
            {
                if (header.Name == "Subject")
                {
                    return header.Value;
                }
            }

            return null;
        }

        public static string GetBody(Message message)
        {
            var corpohtml = "";
            var corpotext = "";
            var retorno = "";

            if (message == null || message.Payload == null) return null;

            if (message.Payload.Body.Data != null)
            {
                Console.WriteLine("message.Payload.Body.Data != null");
                if (message.Payload.MimeType == "text/html") corpohtml = UtilEmailServices.Base64UrlDecode(message.Payload.Body.Data);
                else corpotext = UtilEmailServices.Base64UrlDecode(message.Payload.Body.Data);
            }
            else
            {
                if (message.Payload.Parts != null)
                {
                    Console.WriteLine("message.Payload.Parts != null");
                    foreach (var part in message.Payload.Parts)
                    {
                        if (part.Body.Data != null)
                        {
                            if (part.MimeType == "text/html") corpohtml = UtilEmailServices.Base64UrlDecode(part.Body.Data);
                            else corpotext = UtilEmailServices.Base64UrlDecode(part.Body.Data);
                        }
                        else
                        {
                            if (part.Parts != null)
                            {
                                foreach (var item in part.Parts)
                                {
                                    if (item.Body.Data != null)
                                    {
                                        if (item.MimeType == "text/html") corpohtml = UtilEmailServices.Base64UrlDecode(item.Body.Data);
                                        else corpotext = UtilEmailServices.Base64UrlDecode(item.Body.Data);

                                    }
                                    else
                                    {
                                        if (item.Parts != null)
                                        {
                                            foreach (var it in item.Parts)
                                            {
                                                if (it.Body.Data != null)
                                                {
                                                    if (it.MimeType == "text/html") corpohtml = UtilEmailServices.Base64UrlDecode(it.Body.Data);
                                                    else corpotext = UtilEmailServices.Base64UrlDecode(it.Body.Data);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

            if (!string.IsNullOrEmpty(corpohtml)) retorno = corpohtml; else retorno = corpotext;

            return retorno;
        }

        public static string GetHeader(Message message, string headerName)
        {
            if (message == null || message.Payload == null || message.Payload.Headers == null)
                return null;

            foreach (var header in message.Payload.Headers)
            {
                if (header.Name == headerName)
                {
                    return header.Value;
                }
            }

            return null;
        }

        static string GetDecodedBody(string data)
        {
            if (string.IsNullOrEmpty(data)) return "";

            var codedBody = data.Replace("-", "+").Replace("_", "/");
            var base64EncodedBytes = Convert.FromBase64String(codedBody);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static string Base64UrlToBase64(string base64Url)
        {
            // Substituir os caracteres específicos do Base64url ('-' => '+', '_' => '/')
            string base64 = base64Url.Replace('-', '+').Replace('_', '/');

            // A string Base64 precisa ter um comprimento múltiplo de 4, então adicionar '=' conforme necessário
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return base64;
        }

    }
}
