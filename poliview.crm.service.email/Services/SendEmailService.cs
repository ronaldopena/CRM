using Poliview.crm.domain;

namespace poliview.crm.service.email.Services
{
    public interface ISendEmailService
    {
        public void SendEmailAsync(IEnumerable<Email> emailsParaEnvio, ContaEmail conta, Serilog.ILogger log);
        public void EnviarEmailAvulso(string destinatarios, string assunto, string corpo, ContaEmail conta, Serilog.ILogger log);
    }    
}
