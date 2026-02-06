using MailKit.Security;
using MimeKit;
using Poliview.crm.domain;
using Poliview.crm.services;
using Poliview.crm.repositorios;

namespace Poliview.crm.service.email.Services
{
    public class SendEmailPadraoService : poliview.crm.service.email.Services.ISendEmailService
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
        private static string _tituloMensagem = "Envio de Email POP/SMTP PADRÃO";

        public IEnumerable<Email> EmailsParaEnviar { get; set; }
        public SendEmailPadraoService(IConfiguration _configuration, LogService logService, INotificacaoErro notificacaoErro)
        {
            configuration = _configuration;
            _logService = logService;
            _notificacaoErro = notificacaoErro;
            connection = _configuration["conexao"].ToString();
            _cliente = configuration["cliente"] ?? "não identificado";
            verQuery = Convert.ToBoolean(configuration["verQuery"]);
            verDebug = Convert.ToBoolean(configuration["verDebug"]);
            verErros = Convert.ToBoolean(configuration["verErros"]);
            verInfos = Convert.ToBoolean(configuration["verInfos"]);
        }

        public async Task EnviarEmailAvulsoAsync(string destinatarios, string assunto, string corpo, ContaEmail conta, Serilog.ILogger log)
        {
            var emailLista = UtilEmailServices.ExtrairListaEmails(destinatarios);

            if (emailLista.Length > 0 && !String.IsNullOrEmpty(destinatarios))
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(conta.nomeRemetente, conta.emailRemetente));
                message.Subject = assunto;

                var count = 0;

                foreach (var destinatario in emailLista)
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
                    // client.Connect(config.hostsmtp, config.portsmtp, SecureSocketOptions.StartTls);
                    Console.WriteLine("Email Avulso");
                    Console.WriteLine(conta.hostsmtp);
                    client.Connect(conta.hostsmtp, conta.portasmtp, SecureSocketOptions.None);

                    client.Authenticate(conta.usuario, conta.senha);
                    client.Send(message);
                    client.Disconnect(true);
                }

                log.Debug($"EMAIL AVULSO enviado com sucesso! {destinatarios} - {conta.descricaoConta}");

            }

        }

        public async void SendEmailAsync(IEnumerable<Email> emailsParaEnvio,
                                         ContaEmail conta,
                                         Serilog.ILogger log)
        {
            try
            {
                var chamadoService = new ChamadoService(configuration);

                var emailService = new EmailService(configuration, log);
                var idatual = 0;

                foreach (var email in emailsParaEnvio)
                {
                    try
                    {
                        idatual = email.id;

                        var strassunto = "";

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

                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress(conta.nomeRemetente, conta.emailRemetente));

                        var count = 0;
                        var e = new MailboxAddress("", "");

                        foreach (var destinatario in emailLista)
                        {

                            if (!string.IsNullOrEmpty(conta.emaildestinatariosuporte))
                            {
                                strassunto = " enviado para: " + conta.emaildestinatariosuporte + " em nome de: " + destinatario;
                            }
                            else
                            {
                                strassunto = "";
                            }

                            email.assunto = email.assunto + strassunto;

                            email.assunto = UtilEmailServices.TrocaVariaveisTexto(email.idchamado, email.idocorrencia, email.assunto, configuration);
                            email.corpo = UtilEmailServices.TrocaVariaveisTexto(email.idchamado, email.idocorrencia, email.corpo, configuration);
                            emailService.SalvarCorpoEAssunto(email.id, email.corpo, email.assunto, email.idaviso);

                            var dadosenvioemail = $"{conta.descricaoConta} - enviar email para {destinatario} assunto: {email.assunto} ";
                            log.Debug(dadosenvioemail);

                            message.Subject = email.assunto;

                            if (!string.IsNullOrEmpty(conta.emaildestinatariosuporte))
                                e = new MailboxAddress("", conta.emaildestinatariosuporte);
                            else
                                e = new MailboxAddress("", destinatario);

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
                        builder.HtmlBody = email.corpo;

                        if (!string.IsNullOrEmpty(email.urlanexo))
                        {
                            if (System.IO.File.Exists(email.urlanexo))
                            {
                                byte[] contentBytes = System.IO.File.ReadAllBytes(email.urlanexo);
                                var nomearquivo = Path.GetFileName(email.urlanexo);

                                var anexo = new MimePart("application", "octet-stream")
                                {
                                    Content = new MimeKit.MimeContent(new MemoryStream(contentBytes)),
                                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                                    ContentTransferEncoding = ContentEncoding.Base64,
                                    FileName = nomearquivo
                                };

                                builder.Attachments.Add(anexo);
                            }
                            else
                            {
                                email.erroenvio = $"{conta.descricaoConta} - arquivo {email.urlanexo} não encontrado.";
                            }

                        }
                        else
                        {
                            var attachments = emailService.ListarAnexosEmail(email.id, log);
                            foreach (var attachment in attachments)
                            {
                                if (attachment.arquivo != null)
                                {
                                    builder.Attachments.Add(attachment.nome + attachment.extensao, attachment.arquivo);
                                }

                            }
                        }

                        // Define o corpo do email
                        message.Body = builder.ToMessageBody();
                        // message.To.Append(emailPara);

                        try
                        {
                            using (var client = new MailKit.Net.Smtp.SmtpClient())
                            {
                                client.Connect(conta.hostsmtp, conta.portasmtp, SecureSocketOptions.None);
                                // client.Connect(conta.hostsmtp, 587, SecureSocketOptions.None);
                                // client.Connect(conta.hostsmtp, conta.portasmtp, SecureSocketOptions.StartTls);
                                // client.Connect(conta.hostsmtp, conta.portasmtp, SecureSocketOptions.StartTls);
                                // client.Connect(conta.hostsmtp, conta.portasmtp, SecureSocketOptions.None);
                                client.Authenticate(conta.usuario, conta.senha);
                                client.Send(message);
                                client.Disconnect(true);
                            }

                            log.Debug($"{conta.descricaoConta} - email enviado com sucesso! {email.emaildestinatario}");

                            if (email.idchamado > 0)
                            {
                                var emailCliente = chamadoService.EmailDoCliente(email.idchamado);

                                foreach (var destinatario in emailLista)
                                {
                                    if ((emailCliente == destinatario) && (!string.IsNullOrEmpty(emailCliente)))
                                    {
                                        chamadoService.MarcarMensagensChamado(email.idchamado);
                                    }
                                }
                            }

                            emailService.MarcarEmailComoEnviado(email.id, log);
                            log.Debug($"{conta.descricaoConta} - Marcar Email como enviado. Id " + email.id);
                        }
                        catch (Exception ex)
                        {
                            UtilEmailServices.MarcarEmailComErro(email.id, ex.Message, log, connection);
                            log.Error($"{conta.descricaoConta} - Erro: {ex.Message} email: id={email.id} {email.emaildestinatario} {email.assunto} ");
                            var mensagemErro = $"Erro no envio de email - Conta: {conta.descricaoConta}\n\n" +
                                             $"Email ID: {email.id}\n\n" +
                                             $"Destinatário: {email.emaildestinatario}\n\n" +
                                             $"Assunto: {email.assunto}\n\n" +
                                             $"Detalhes: {ex.Message}\n\n" +
                                             (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                            _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
                        }

                    }
                    catch (Exception ex)
                    {
                        log.Error($"{conta.descricaoConta} - Erro: {ex.Message} email: id={email.id} {email.emaildestinatario} {email.assunto} ");
                        var mensagemErro = $"Erro no processamento de email - Conta: {conta.descricaoConta}\n\n" +
                                         $"Email ID: {email.id}\n\n" +
                                         $"Destinatário: {email.emaildestinatario}\n\n" +
                                         $"Assunto: {email.assunto}\n\n" +
                                         $"Detalhes: {ex.Message}\n\n" +
                                         (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                        _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
                        continue;
                    }

                }
                // return Task.FromResult(true);

            }
            catch (Exception ex)
            {
                log.Error($"{conta.descricaoConta} - Erro: {ex.Message}");
                var mensagemErro = $"Erro geral no envio de emails - Conta: {conta.descricaoConta}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
            }
        }

        public void Send(string assunto, string corpo, string emailDestinatario)
        {
            throw new NotImplementedException();
        }
    }
}
