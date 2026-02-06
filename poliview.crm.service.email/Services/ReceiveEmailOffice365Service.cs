using Microsoft.Graph;
using Azure.Identity;
using Poliview.crm.domain;
using poliview.crm.service.email.Services;
using Poliview.crm.services;
using Poliview.crm.repositorios;

namespace Poliview.crm.service.email.Services
{
    public class ReceiveEmailOffice365Service : IReceiveEmailService
    {
        private static string connection = "";
        private static IConfiguration configuration;
        private LogService _logService;
        private static bool verQuery = true;
        private static bool verDebug = true;
        private static bool verErros = true;
        private static bool verInfos = true;
        private string _cliente = "não identificado";
        private static string _tituloMensagem = "Recebimento de Email Office365";

        public ReceiveEmailOffice365Service(IConfiguration _configuration, LogService logService)
        {
            configuration = _configuration;
            _logService = logService;
            _cliente = configuration["cliente"] ?? "não identificado";
            connection = configuration["conexao"].ToString();
            verQuery = Convert.ToBoolean(configuration["verQuery"]);
            verDebug = Convert.ToBoolean(configuration["verDebug"]);
            verErros = Convert.ToBoolean(configuration["verErros"]);
            verInfos = Convert.ToBoolean(configuration["verInfos"]);
        }

        public async Task<bool> ReceiveEmailAsync(Serilog.ILogger log, ContaEmail conta)
        {
            await ReceberEmailOffice365(log, conta);
            ProcessarEmailsRecebidos.Processar(log, configuration, verInfos);
            return true;
        }

        private static async Task DeletarEmailCaixaEntradaAsync(GraphServiceClient graphClient, string UserId, string MessageId, Serilog.ILogger log)
        {
            try
            {
                await graphClient.Users[UserId].Messages[MessageId].Request().DeleteAsync();
                log.Information("mensagem excluida com sucesso!");
            }
            catch (Exception ex)
            {
                var mensagemErro = $"Erro ao deletar email na caixa de entrada  - Conta: {UserId}\n\n" +
                     $"Detalhes: {ex.Message}\n\n" +
                     (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");

                UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);
                log.Error(ex.Message);
            }
        }

        private async Task ReceberEmailOffice365(Serilog.ILogger log, ContaEmail conta)
        {
            var dadosDoEmail = "";
            var msg = "";
            var EmailDoCrm = "";

            try
            {
                log.Information($"{conta.descricaoConta} - RECEBIMENTO DE EMAIL OFFICE365 INICIADO");

                string[] scopes = { "https://graph.microsoft.com/.default" };

                ClientSecretCredential clientSecretCredential = new ClientSecretCredential(conta.tenant_id, conta.client_id, conta.clientSecret);

                GraphServiceClient graphClient = new GraphServiceClient(clientSecretCredential, scopes);

                User me = await graphClient.Users[conta.userId].Request().GetAsync();

                EmailDoCrm = me.Mail;

                if (verDebug)
                {
                    log.Debug($"id....: {me.Id}");
                    log.Debug($"userId: {conta.userId}");
                    log.Debug($"Nome Usuário: {me.DisplayName}");
                    log.Debug($"Email: {me.Mail}");
                }

                var mensagens = await graphClient.Users[conta.userId]
                    .MailFolders["inbox"]
                    .Messages
                    .Request()
                    .Top(conta.qtdeemailsrecebimento)
                    .GetAsync();

                if (verInfos)
                {
                    log.Information($"{conta.descricaoConta} - Foram encontradas {mensagens.Count} mensagens na caixa de entrada!");
                    log.Information("");
                }
                var id = "";
                var email = new Email();
                var emailService = new EmailService(configuration, log);

                var arquivoService = new ArquivoService(configuration);
                var idemailentrada = 0;

                dadosDoEmail = "";
                var idemail = 0;
                var idaviso = 0;
                var listaAnexos = new List<int>();
                var i = 0;
                var emailDoDestinatario = "";
                var TamanhoMaximoAnexosAtingido = false;

                foreach (var mensagem in mensagens)
                {
                    if (mensagem.From == null)
                    {
                        await DeletarEmailCaixaEntradaAsync(graphClient, conta.userId, mensagem.Id, log);
                        continue;
                    }

                    i += 1;
                    log.Information($"{conta.descricaoConta} - RECEBENDO MENSAGEM {i} de {mensagens.Count} de {conta.descricaoConta} ");
                    msg = "";

                    var ids = UtilEmailServices.retornaIdChamadoOcorrencia(mensagem.Subject);
                    email.nomeremetente = "";
                    //email.emailremetente = mensagem.From.EmailAddress.Address.Trim();
                    var remetenteEmail = UtilEmailServices.ExtrairListaEmails(mensagem.From.EmailAddress.Address.Trim());
                    email.emailremetente = remetenteEmail[0].ToString().Trim();
                    email.emaildestinatario = me.Mail.Trim();
                    // email.emaildestinatario = mensagem.ToRecipients.FirstOrDefault().EmailAddress.Address;
                    email.datainclusao = DateTime.Now;
                    email.assunto = mensagem.Subject;
                    email.corpo = mensagem.Body.Content;
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

                    var attachments = await graphClient.Users[conta.userId].Messages[mensagem.Id].Attachments.Request().GetAsync();
                    var TamanhoTotalAnexos = 0;
                    foreach (var anexo in attachments.CurrentPage)
                    {
                        var attachmentRequest = graphClient.Users[conta.userId].Messages[mensagem.Id].Attachments[anexo.Id].Request().GetAsync();
                        var fileAttachment = attachmentRequest.Result as Microsoft.Graph.FileAttachment;
                        if (fileAttachment != null)
                        {
                            TamanhoTotalAnexos += fileAttachment.Size.Value;
                        }
                    }

                    if (TamanhoTotalAnexos > conta.tamanhomaximoanexos * 1024 * 1024)
                    {
                        emailService.EnviarEmailTamanhoMaximoAnexoAtingido(email, log);
                        await DeletarEmailCaixaEntradaAsync(graphClient, conta.userId, mensagem.Id, log);
                        continue;
                    }

                    try
                    {
                        if (id == "") id = mensagem.Id;

                        if (mensagem.From != null && me.Mail.ToLower().Trim() != mensagem.From.EmailAddress.Address.ToLower().Trim())
                        {
                            msg = "";

                            if (email.idocorrencia == 0) email.idocorrencia = 1;

                            dadosDoEmail = $"{conta.descricaoConta} - id: {email.id} | remetente: {email.emailremetente} | destinatário: {email.emaildestinatario} | Assunto: {email.assunto}";

                            idemail = emailService.RetornaIdEmail(email);

                            var confirmacaoentrega = emailService.EmailDeConfirmacaoEntrega(email);
                            var idemailnovo = 0;

                            if (confirmacaoentrega > 0)
                            {
                                await DeletarEmailCaixaEntradaAsync(graphClient, conta.userId, mensagem.Id, log);

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
                                        // var attachments = await graphClient.Users[config.userId].Messages[mensagem.Id].Attachments.Request().GetAsync();
                                        var idanexo = 0;
                                        listaAnexos = new List<int>();
                                        foreach (var anexo in attachments.CurrentPage)
                                        {
                                            if (verDebug) log.Debug($"anexo: {anexo.Name}");
                                            var attachmentRequest = graphClient.Users[conta.userId].Messages[mensagem.Id].Attachments[anexo.Id].Request().GetAsync();
                                            var fileAttachment = attachmentRequest.Result as Microsoft.Graph.FileAttachment;
                                            if (fileAttachment != null)
                                            {
                                                idanexo = 0;
                                                var arquivo = new Arquivo();

                                                var nomeArq = Path.GetFileNameWithoutExtension(anexo.Name);
                                                var extArq = Path.GetExtension(anexo.Name);

                                                if (nomeArq.Length + extArq.Length > 100)
                                                    nomeArq = nomeArq.Substring(0, 99 - extArq.Length);

                                                arquivo.arquivo = nomeArq;
                                                arquivo.extensao = extArq;
                                                arquivo.conteudo = fileAttachment.ContentBytes;
                                                arquivo.banco = "0";
                                                arquivo.padrao = "N";
                                                arquivo.chamado = "S";
                                                arquivo.idusuario = 1;
                                                arquivo.data = DateTime.Now;
                                                if (!anexo.Name.ToLower().Contains("image001") && !anexo.Name.ToLower().Contains("image002"))
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
                                else
                                {
                                    idemailnovo = emailService.SalvarQuarentena(email, log, ref msg);
                                    log.Debug($"Não é email do CRM: {idemailnovo} ");
                                }
                                await DeletarEmailCaixaEntradaAsync(graphClient, conta.userId, mensagem.Id, log);
                            }

                        }
                        else
                        {
                            if (verInfos) log.Information($"{conta.descricaoConta} - Mensagem Excluída! {mensagem.Id}");
                            await DeletarEmailCaixaEntradaAsync(graphClient, conta.userId, mensagem.Id, log);
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

                        var mensagemErro = $"Erro ao receber email - Conta: {conta.descricaoConta}\n\n" +
                                         $"Detalhes: {ex.Message}\n\n" +
                                         (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "") +
                                         $"\n\nDados do Email: {dadosDoEmail}";

                        UtilEmailServices.NotificarErro(_tituloMensagem, mensagemErro, configuration);

                        log.Error($"{conta.descricaoConta} - ERRO AO RECEBER EMAIL MSG " + msg);

                    }

                }
                if (verInfos) log.Information($"{conta.descricaoConta} - RECEBIMENTO DE EMAIL FINALIZADO " + conta.descricaoConta);
            }
            catch (Exception ex)
            {
                log.Error($"{conta.descricaoConta} - ERRO DE CONEXÃO COM O OFFICE 365 ReceiveEmailService.cs(461) " + conta.descricaoConta);
                log.Error($"{conta.descricaoConta} - {ex.Message} - {dadosDoEmail}");

                // Enviar notificação via Telegram
                try
                {
                    var mensagemErro = $"Erro ao receber email - Conta: {conta.descricaoConta}\n\n" +
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
}
