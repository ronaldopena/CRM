using Azure.Identity;
using Microsoft.Graph;
using Poliview.crm.domain;
using Poliview.crm.services;
using Poliview.crm.repositorios;

namespace Poliview.crm.service.email.Services
{
    public class SendEmailOffice365Service : poliview.crm.service.email.Services.ISendEmailService
    {
        private static string connection = "";
        private static IConfiguration configuration;
        private LogService _logService;
        private static bool verQuery = true;
        private static bool verDebug = true;
        private static bool verErros = true;
        private static bool verInfos = true;
        private string _cliente = "não identificado";
        private static string _tituloMensagem = "Envio de Email OFFICE365";
        public IEnumerable<Email> EmailsParaEnviar { get; set; }
        public SendEmailOffice365Service(IConfiguration _configuration, LogService logService)
        {
            configuration = _configuration;
            _logService = logService;
            connection = configuration["conexao"].ToString();
            _cliente = configuration["cliente"] ?? "não identificado";
            verQuery = Convert.ToBoolean(configuration["verQuery"]);
            verDebug = Convert.ToBoolean(configuration["verDebug"]);
            verErros = Convert.ToBoolean(configuration["verErros"]);
            verInfos = Convert.ToBoolean(configuration["verInfos"]);
        }

        public async void EnviarEmailAvulso(string destinatarios, string assunto, string corpo, ContaEmail conta, Serilog.ILogger log)
        {

            string[] scopes = { "https://graph.microsoft.com/.default" };
            ClientSecretCredential clientSecretCredential = new ClientSecretCredential(conta.tenant_id, conta.client_id, conta.clientSecret);
            GraphServiceClient graphClient = new GraphServiceClient(clientSecretCredential, scopes);
            User me = await graphClient.Users[conta.userId].Request().GetAsync();
            var nomeRemetente = me.DisplayName;
            var emailRemetente = me.Mail;

            string[] emailLista = UtilEmailServices.ExtrairListaEmails(destinatarios);

            try
            {

                var emailPara = new List<Recipient>();
                var emailComCopia = new List<Recipient>();
                var count = 0;

                foreach (var destinatario in emailLista)
                {
                    var e = new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = destinatario.Trim(),
                        }
                    };

                    if (count == 0)
                    {
                        emailPara.Add(e);
                    }
                    else
                    {
                        emailComCopia.Add(e);
                    }
                    count++;
                }

                var message = new Message
                {
                    IsDeliveryReceiptRequested = true,
                    IsReadReceiptRequested = true,
                    Sender = new Recipient()
                    {
                        EmailAddress = new EmailAddress
                        {
                            Name = nomeRemetente,
                            Address = emailRemetente.Trim(),
                        }
                    },
                    Subject = assunto,
                    Importance = Importance.High,
                    Categories = new List<String>()
                    {
                        "CRM Poliview"
                    },
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = corpo
                    },
                    ToRecipients = emailPara,
                    CcRecipients = emailComCopia,
                };

                var saveToSentItems = true;

                try
                {
                    await graphClient.Users[conta.userId]
                        .SendMail(message, saveToSentItems)
                        .Request()
                        .PostAsync();

                    log.Debug($"Enviada mensagem {message.ToString()} - {conta.descricaoConta}");
                }
                catch (Exception ex)
                {
                    log.Error($"Erro(305-SendEmailService.cs): {ex.Message} - {conta.descricaoConta}");

                    // Enviar notificação via Telegram
                    try
                    {
                        var mensagemErro = $"Erro no envio de email avulso - Conta: {conta.descricaoConta}\n\n" +
                                         $"Detalhes: {ex.Message}\n\n" +
                                         (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                        UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);
                    }
                    catch (Exception telegramEx)
                    {
                        await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                            $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Erro(310-SendEmailService.cs): {ex.Message} - {conta.descricaoConta}");

                // Enviar notificação via Telegram
                try
                {
                    var mensagemErro = $"Erro geral no envio de email avulso - Conta: {conta.descricaoConta}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                    UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);
                }
                catch (Exception telegramEx)
                {
                    await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                        $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                }
            }
            finally
            {
                if (verInfos) log.Information($"Processo Finalizado - {conta.descricaoConta}");
            }
            // return true;
        }

        public async void SendEmailAsync(IEnumerable<Email> emailsParaEnvio, ContaEmail conta, Serilog.ILogger log)
        {
            await EnviarEmailOffice365(emailsParaEnvio, conta, log);
        }

        private async Task EnviarEmailOffice365(IEnumerable<Email> emailsParaEnvio, ContaEmail conta, Serilog.ILogger log)
        {
            // if (verDebug) log.Debug(connection);

            var emailService = new EmailService(configuration, log);
            var idatual = 0;

            try
            {

                string[] scopes = { "https://graph.microsoft.com/.default" };

                ClientSecretCredential clientSecretCredential = new ClientSecretCredential(conta.tenant_id, conta.client_id, conta.clientSecret);

                GraphServiceClient graphClient = new GraphServiceClient(clientSecretCredential, scopes);

                User me = await graphClient.Users[conta.userId].Request().GetAsync();

                if (verDebug) log.Debug($"{conta.descricaoConta} - Email: {me.Mail}");

                var chamadoService = new ChamadoService(configuration);

                var i = 1;
                var tot = emailsParaEnvio.Count();

                foreach (var email in emailsParaEnvio)
                {
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

                    try
                    {
                        var emailPara = new List<Recipient>();
                        var emailComCopia = new List<Recipient>();
                        var count = 0;
                        var listaDestinatarios = "";
                        var assunto = "";

                        foreach (var destinatario in emailLista)
                        {
                            listaDestinatarios += destinatario + " ";

                            if (string.IsNullOrEmpty(conta.emaildestinatariosuporte))
                            {
                                var e = new Recipient
                                {
                                    EmailAddress = new EmailAddress
                                    {
                                        Address = destinatario.Trim(),
                                    }
                                };

                                if (count == 0)
                                {
                                    emailPara.Add(e);
                                }
                                else
                                {
                                    emailComCopia.Add(e);
                                }
                                count++;
                            }
                            else
                            {
                                var eAlternativo = new Recipient
                                {
                                    EmailAddress = new EmailAddress
                                    {
                                        Address = conta.emaildestinatariosuporte.Trim(),
                                    }
                                };

                                if (emailPara.IndexOf(eAlternativo) == -1)
                                    emailPara.Add(eAlternativo);
                            }

                        }

                        if (!string.IsNullOrEmpty(conta.emaildestinatariosuporte))
                        {
                            strassunto = " em nome de: " + listaDestinatarios;
                        }
                        else
                        {
                            strassunto = "";
                        }

                        email.assunto = email.assunto + strassunto;

                        email.assunto = UtilEmailServices.TrocaVariaveisTexto(email.idchamado, email.idocorrencia, email.assunto, configuration);
                        email.corpo = UtilEmailServices.TrocaVariaveisTexto(email.idchamado, email.idocorrencia, email.corpo, configuration);
                        emailService.SalvarCorpoEAssunto(email.id, email.corpo, email.assunto, email.idaviso);

                        dadosenvioemail = $"{conta.descricaoConta} - enviar email para {listaDestinatarios} assunto: {email.assunto} ";
                        log.Debug(dadosenvioemail);

                        var message = new Message
                        {
                            IsDeliveryReceiptRequested = true,
                            IsReadReceiptRequested = true,
                            Sender = new Recipient()
                            {
                                EmailAddress = new EmailAddress
                                {
                                    Name = email.nomeremetente,
                                    Address = email.emailremetente.Trim(),
                                }
                            },
                            Subject = email.assunto,
                            Importance = Importance.High,
                            Categories = new List<String>()
                            {
                                "CRM Poliview"
                            },
                            Body = new ItemBody
                            {
                                ContentType = BodyType.Html,
                                Content = email.corpo
                            },
                            ToRecipients = emailPara,
                            CcRecipients = emailComCopia,
                            InternetMessageHeaders = new List<InternetMessageHeader>
                            {
                                new InternetMessageHeader
                                {
                                    Name = "X-idemailcrm",
                                    Value = email.id.ToString()
                                },
                                new InternetMessageHeader
                                {
                                    Name = "X-idchamadocrm",
                                    Value = email.idchamado.ToString()
                                }
                            }
                        };

                        var anexos = emailService.ListarAnexosEmail(email.id, log);

                        string contentType = "application/octet-stream";
                        MessageAttachmentsCollectionPage attachments = new MessageAttachmentsCollectionPage();

                        if (!string.IsNullOrEmpty(email.urlanexo))
                        {
                            if (System.IO.File.Exists(email.urlanexo))
                            {
                                byte[] contentBytes = System.IO.File.ReadAllBytes(email.urlanexo);
                                attachments.Add(new FileAttachment
                                {
                                    ODataType = "#microsoft.graph.fileAttachment",
                                    ContentBytes = contentBytes,
                                    ContentType = contentType,
                                    ContentId = "testing",
                                    Name = Path.GetFileName(email.urlanexo)
                                });
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
                                attachments.Add(new FileAttachment
                                {
                                    ODataType = "#microsoft.graph.fileAttachment",
                                    ContentBytes = contentBytes,
                                    ContentType = contentType,
                                    ContentId = "testing",
                                    Name = anexo.nome + anexo.extensao
                                });
                            }
                            message.Attachments = attachments;
                        }
                        if (attachments.Count > 0) message.Attachments = attachments;

                        var saveToSentItems = true;

                        try
                        {
                            email.erroenvio = "";
                            var result = graphClient.Users[conta.userId].SendMail(message, saveToSentItems);


                            var response = await graphClient.Users[conta.userId]
                                .SendMail(message, saveToSentItems)
                                .Request()
                                .PostResponseAsync();

                            string locationHeader = response.HttpHeaders.ToString();
                            var messageId = response.HttpHeaders.GetValues("request-id").FirstOrDefault();
                            var statusCode = response.StatusCode.ToString();

                            // var idMensagem = message.Id;

                            // var str = message.InternetMessageId;
                            // var str1 = message.WebLink;

                            // var messageResponse = await graphClient.Users[conta.userId].Messages[messageId].Request().GetAsync();

                            log.Debug($"{conta.descricaoConta} - Enviada mensagem " + message.ToString());

                            if (email.idchamado > 0)
                            {
                                var emailCliente = chamadoService.EmailDoCliente(email.idchamado);

                                foreach (var destinatario in emailPara)
                                {
                                    if ((emailCliente == destinatario.EmailAddress.ToString()) && (!string.IsNullOrEmpty(emailCliente)))
                                    {
                                        chamadoService.MarcarMensagensChamado(email.idchamado);
                                    }
                                }
                            }

                            emailService.MarcarEmailComoEnviado(email.id, log, email.erroenvio, messageId, statusCode);
                            log.Debug($"{conta.descricaoConta} - Marcar Email como enviado. Id " + email.id);
                        }
                        catch (Exception ex)
                        {
                            UtilEmailServices.MarcarEmailComErro(email.id, ex.Message, log, connection);
                            log.Error($"{conta.descricaoConta} - Erro: {ex.Message} email: id={email.id} {email.emaildestinatario} {email.assunto} - {conta.descricaoConta}");

                            // Enviar notificação via Telegram
                            try
                            {
                                var mensagemErro = $"Erro no envio de email - Conta: {conta.descricaoConta}\n\n" +
                                                 $"Email ID: {email.id}\n\n" +
                                                 $"Destinatário: {email.emaildestinatario}\n\n" +
                                                 $"Assunto: {email.assunto}\n\n" +
                                                 $"Detalhes: {ex.Message}\n\n" +
                                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                                UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);
                            }
                            catch (Exception telegramEx)
                            {
                                await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                                    $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{conta.descricaoConta} - Erro(586-SendEmailService.cs): {ex.Message} - {conta.descricaoConta}");
                        UtilEmailServices.MarcarEmailComErro(idatual, ex.Message, log, connection);
                        log.Debug($"{conta.descricaoConta} - ERRO AO ENVIAR EMAIL: " + dadosenvioemail);

                        // Enviar notificação via Telegram
                        try
                        {
                            var mensagemErro = $"Erro no processamento de email - Conta: {conta.descricaoConta}\n\n" +
                                             $"Email ID: {idatual}\n\n" +
                                             $"Detalhes: {ex.Message}\n\n" +
                                             (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                            UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);
                        }
                        catch (Exception telegramEx)
                        {
                            await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                                $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                if (verErros) log.Error($"{conta.descricaoConta} - Erro: {ex.Message} ");
                UtilEmailServices.MarcarEmailComErro(idatual, ex.Message, log, connection);
                if (verDebug) log.Debug($"{conta.descricaoConta} - Marcar Email com Erro. Id " + idatual);

                // Enviar notificação via Telegram
                try
                {
                    var mensagemErro = $"Erro geral no envio de emails - Conta: {conta.descricaoConta}\n\n" +
                                     $"Detalhes: {ex.Message}\n\n" +
                                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                    UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);
                }
                catch (Exception telegramEx)
                {
                    await _logService.Log(LogRepository.OrigemLog.integracao, LogRepository.TipoLog.erro,
                        $"Erro ao enviar notificação Telegram: {telegramEx.Message}");
                }
            }
            finally
            {
                if (verInfos) log.Information($"{conta.descricaoConta} - Processo Finalizado");
            }
        }

        public void Send(string assunto, string corpo, string emailDestinatario)
        {
            throw new NotImplementedException();
        }
    }


}
