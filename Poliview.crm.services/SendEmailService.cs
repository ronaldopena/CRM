using Dapper;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using Microsoft.Data.SqlClient;
using MailKit.Net.Smtp;

namespace Poliview.crm.services
{
    public interface ISendEmailService
    {
        public void Send(string assunto, string corpo, string emailDestinatario);
    }

    public class ConfigEmail
    {
        public string? emailRemetente { get; set; }
        public string? servidorSmtp { get; set; }
        public int portaSmtp { get; set; }
        public string? usuario { get; set; }
        public string? senha { get; set; }
    }

    public class SendEmailService : ISendEmailService
    {
        private readonly string _connectionString;
        private IConfiguration _configuration;

        public SendEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration["conexao"];
        }

        public void Send(string assunto, string corpo, string emailDestinatario)
        {
            var config = this.Config();
            var emailobj = new MimeMessage();
            emailobj.From.Add(MailboxAddress.Parse(config.emailRemetente));
            emailobj.To.Add(MailboxAddress.Parse(emailDestinatario));
            emailobj.Subject = assunto;
            emailobj.Body = new TextPart(TextFormat.Html) { Text = corpo };
            using var smtp = new SmtpClient();
            smtp.Connect(config.servidorSmtp, config.portaSmtp, SecureSocketOptions.StartTls);
            smtp.Authenticate(config.usuario, config.senha);
            smtp.Send(emailobj);
            smtp.Disconnect(true);            
        }

        private ConfigEmail Config()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = $"select DS_EmailFrom as emailRemetente, DS_EmailHost as servidorSmtp, NR_EmailPort as portaSmtp, DS_EmailUserName as usuario, DS_EmailPassword as senha from OPE_PARAMETRO where CD_BancoDados = 1 and CD_Mandante = 1";
            return connection.QueryFirst<ConfigEmail>(query);
        }

    }
}
