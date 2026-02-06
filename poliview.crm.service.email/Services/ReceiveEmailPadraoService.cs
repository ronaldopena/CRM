using MailKit.Net.Pop3;
using MimeKit;
using Poliview.crm.domain;
using Poliview.crm.service.email.Services;
using poliview.crm.service.email.Services;
using Poliview.crm.services;
using MailKit.Security;
using Poliview.crm.repositorios;

namespace Poliview.crm.service.email.Services
{
    public class ReceiveEmailPadraoService : IReceiveEmailService
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
        private static string _tituloMensagem = "Recebimento de Email Padrão";

        public ReceiveEmailPadraoService(IConfiguration _configuration, LogService logService, INotificacaoErro notificacaoErro)
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

        public async Task<bool> ReceiveEmailAsync(Serilog.ILogger log, ContaEmail conta)
        {
            var chamadoService = new ChamadoService(configuration);

            await ReceberEmailPadrao(log, conta);
            ProcessarEmailsRecebidos.Processar(log, configuration, verInfos);

            return true;
        }

        private async Task ReceberEmailPadrao(Serilog.ILogger log, ContaEmail conta)
        {
            log.Information($"{conta.descricaoConta} - RECEBIMENTO DE EMAIL PADRÃO INICIADO");

            var id = "";
            var emailService = new EmailService(configuration, log);
            var arquivoService = new ArquivoService(configuration);
            var idemailentrada = 0;
            var dadosDoEmail = "antes do inicio do processo";
            var idemailnovo = 0;
            var idaviso = 0;
            var listaAnexos = new List<int>();

            try
            {
                using (var client = new Pop3Client())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    if (conta.portapop == 110)
                    {
                        client.Connect(conta.hostpop, conta.portapop, SecureSocketOptions.None);
                    }
                    else
                    {
                        client.Connect(conta.hostpop, conta.portapop, conta.sslpop);
                    }

                    client.Authenticate(conta.usuario, conta.senha);

                    var emailsNaCaixaEntrada = client.Count;

                    if (client.Count > conta.qtdeemailsrecebimento) emailsNaCaixaEntrada = conta.qtdeemailsrecebimento;

                    for (int i = 0; i < emailsNaCaixaEntrada; i++)
                    {
                        var email = new Email();

                        var msg = "";
                        try
                        {
                            var message = client.GetMessage(i);

                            var ids = UtilEmailServices.retornaIdChamadoOcorrencia(message.Subject);

                            var listaRemetentes = message.From;
                            var listaDestinatarios = message.To;

                            var remetenteEmail = UtilEmailServices.ExtrairListaEmails(listaRemetentes[0].ToString().Trim());


                            if (listaDestinatarios.Count == 0)
                            {
                                if (verDebug) log.Debug($"Não existe destinatário. Mensagem Excluida: {dadosDoEmail}");
                                client.DeleteMessage(i);
                                continue;
                            }

                            var destinatarioEmail = UtilEmailServices.ExtrairListaEmails(listaDestinatarios[0].ToString().Trim());

                            email.classificacaoemail = 7; // email recebido
                            email.idcontaemail = conta.id;
                            email.emailremetente = remetenteEmail[0].ToString().Trim();
                            email.emaildestinatario = destinatarioEmail[0].ToString().Trim();
                            email.nomeremetente = "";
                            email.datainclusao = DateTime.Now;
                            email.assunto = message.Subject;
                            if (message.HtmlBody != null)
                            {
                                email.corpo = message.HtmlBody.ToString();
                                email.corpohtml = 1;
                            }
                            else
                            {
                                email.corpohtml = 0;
                                if (message.Body != null)
                                    email.corpo = message.Body.ToString();
                                else
                                    email.corpo = "";
                            }
                            email.idstatusenvio = 0;
                            email.dataenvio = DateTime.MinValue;
                            email.tipoemail = "R";
                            email.erroenvio = "";
                            email.idchamado = Convert.ToInt16(ids[0].ToString());
                            email.idocorrencia = Convert.ToInt16(ids[1].ToString());
                            email.entregue = 0;
                            email.processado = 0;
                            email.urlanexo = "";
                            if (email.idocorrencia == 0) email.idocorrencia = 1;

                            long TamanhoTotalAnexos = 0;
                            foreach (var attachment in message.Attachments)
                            {
                                if (attachment is MimePart)
                                {
                                    TamanhoTotalAnexos += ((MimePart)attachment).Content.Stream.Length;
                                }
                            }

                            if (TamanhoTotalAnexos > conta.tamanhomaximoanexos * 1024 * 1024)
                            {
                                emailService.EnviarEmailTamanhoMaximoAnexoAtingido(email, log);
                                client.DeleteMessage(i);
                                continue;
                            }

                            var idemail = emailService.RetornaIdEmail(email);
                            var confirmacaoentrega = emailService.EmailDeConfirmacaoEntrega(email);
                            idemailnovo = 0;
                            dadosDoEmail = $"conta: {conta.descricaoConta} remetente: {email.emailremetente} | destinatário: {email.emaildestinatario} | Assunto: {email.assunto} ";

                            if (confirmacaoentrega > 0)
                            {
                                client.DeleteMessage(i);
                                if (confirmacaoentrega == 1)
                                    emailService.MarcarEmailComoConfirmadoProvedor(email, log);
                                else
                                    emailService.MarcarEmailComoRecusadoProvedor(email, log);

                                if (verInfos) log.Information($"CONFIRMAÇÃO EMAIL {dadosDoEmail} - {conta.descricaoConta}");
                            }
                            else
                            {
                                // verifica se é um email para o CRM mesmo. Se não tiver número de chamado no assunto deve ser excluído
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

                                    if (verDebug) log.Debug($"Email Recebido: {dadosDoEmail} - {conta.descricaoConta}");
                                    idemailnovo = emailService.Salvar(email, log, ref msg);
                                    email.id = idemailnovo;
                                    idemailentrada = idemailnovo;
                                    listaAnexos = new List<int>();
                                    foreach (var attachment in message.Attachments)
                                    {
                                        var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                                        using (var stream = new MemoryStream())
                                        {
                                            if (attachment is MessagePart)
                                            {
                                                var rfc822 = (MessagePart)attachment;
                                                rfc822.Message.WriteTo(stream);
                                            }
                                            else
                                            {
                                                var part = (MimePart)attachment;
                                                part.Content.DecodeTo(stream);
                                            }

                                            var arquivo = new Arquivo();
                                            arquivo.arquivo = Path.GetFileNameWithoutExtension(fileName);
                                            arquivo.extensao = Path.GetExtension(fileName);
                                            arquivo.conteudo = stream.ToArray();
                                            arquivo.banco = "0";
                                            arquivo.padrao = "N";
                                            arquivo.chamado = "S";
                                            arquivo.idusuario = 1;
                                            arquivo.data = DateTime.Now;
                                            var idanexo = arquivoService.Salvar(arquivo, log, ref msg);

                                            // não grava imagem da assinatura
                                            if (fileName.ToLower() != "image001.png" && fileName.ToLower() != "image001.jpg")
                                            {
                                                listaAnexos.Add(idanexo);
                                                arquivoService.SalvarAnexoChamado(email.idchamado, email.idocorrencia, idanexo, arquivo.arquivo + arquivo.extensao, log, ref msg);
                                            }


                                            if (idaviso > 0)
                                                emailService.SalvarAvisoEmail(email.id, idanexo, idaviso);
                                        }
                                    }
                                }
                                else
                                {
                                    idemailnovo = emailService.SalvarQuarentena(email, log, ref msg);
                                    if (verDebug) log.Debug($"Email não é do CRM: {idemailnovo}");
                                }
                                client.DeleteMessage(i);
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
                                                (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                            _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);

                            log.Error($"{conta.descricaoConta} - ERRO AO RECEBER EMAIL MSG " + msg);
                            log.Error("ERRO NO LOOP RECEBER EMAIL " + ex.Message + " " + dadosDoEmail + " " + conta.descricaoConta);
                        }
                    }

                    client.Disconnect(true);
                }

            }
            catch (Exception ex)
            {
                log.Error("ERRO AO RECEBER EMAIL " + ex.Message + " " + dadosDoEmail);
                var mensagemErro = $"Erro ao receber email - Conta: {conta.descricaoConta}\n\n" +
                                 $"Detalhes: {ex.Message}\n\n" +
                                 (ex.InnerException != null ? $"Inner Exception: {ex.InnerException.Message}" : "");
                _notificacaoErro.NotificarErro(_tituloMensagem, mensagemErro);
            }
        }
    }

}
