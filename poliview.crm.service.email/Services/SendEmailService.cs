using Poliview.crm.domain;

namespace poliview.crm.service.email.Services
{
    public interface ISendEmailService
    {
        void SendEmailAsync(IEnumerable<Email> emailsParaEnvio, ContaEmail conta, Serilog.ILogger log);
        Task EnviarEmailAvulsoAsync(string destinatarios, string assunto, string corpo, ContaEmail conta, Serilog.ILogger log);
    }    
}
