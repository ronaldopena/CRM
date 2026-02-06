namespace Poliview.crm.domain
{    
    public class ConfigEmailPadrao
    {
        public string nome { get; set; }
        public string email { get; set; }
        public string usuario { get; set; }
        public string senha { get; set; }
        public int smtp { get; set; }
        public string hostsmtp { get; set; }
        public int portsmtp { get; set; }
        public int smtpssl { get; set; }
        public int intervalosmtp { get; set; }
        public int pop { get; set; }
        public string hostpop { get; set; }
        public int portpop { get; set; }
        public int sslpop { get; set; }
        public int intervalopop { get; set; }
        public int qtdemailenviosmtp { get; set; }
        public int maximotentativasenvio { get; set; }
        public string emailalternativo { get; set; }

    }
}
