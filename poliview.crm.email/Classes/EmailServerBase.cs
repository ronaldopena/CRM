using Poliview.crm.domain;
using Poliview.crm.services;

namespace poliview.crm.email.Classes
{
    public class EmailServerBase
    {
        public static void ProcessarEmailsRecebidos(EmailService emailService, Serilog.ILogger log, bool verInfos)
        {
            if (verInfos) log.Information($"PROCESSAMENTO DE EMAIL INICIADO");            
            var emails = emailService.PendentesParaProcessamento(log);
            if (verInfos) log.Information($"Foram encontradas {emails.Count} emails para processamento!");
            foreach (var email in emails)
            {
                var emailcrm = emailService.EmailCrm(email);
                var emailconfirmacao = emailService.EmailDeConfirmacaoEntrega(email);

                var dadosDoEmail = $" idEmail: {email.id} | remetente: {email.emailremetente} | destinatário: {email.emaildestinatario} | Assunto: {email.assunto} EmailCrm={emailcrm} EmailConfirmacao={emailconfirmacao}";

                if (emailconfirmacao > 0)
                {
                    var idemail = emailService.RetornaIdEmail(email);

                    if (emailconfirmacao == 1)
                        emailService.MarcarEmailComoConfirmadoProvedor(email, log);
                    else
                        emailService.MarcarEmailComoRecusadoProvedor(email, log);

                    emailService.Excluir(email.id, log);
                    if (verInfos) log.Information($"CONFIRMAÇÃO EMAIL {dadosDoEmail}");
                }
                else
                {
                    var emailParaChamadoConcluido = emailService.VerificaEmailParaChamadoConcluido(email, 1, log);

                    if (!emailParaChamadoConcluido && emailcrm)
                    {
                        var msg = "";
                        if (emailService.ReplicarEmailParaEnvolvidosComChamado(email, log, ref msg))
                            emailService.MarcarEmailComoProcessado(email.id, log);
                    }
                    else
                    {
                        emailService.Excluir(email.id, log);
                        if (verInfos) log.Information($"EMAIL EXCLUIDO {dadosDoEmail}");
                    }
                }
            }
            if (verInfos) log.Information($"PROCESSAMENTO DE EMAIL FINALIZADO");
        }
    }
}
