using poliview.crm.email.Interfaces;
using poliview.crm.email.Providers;
using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poliview.crm.email
{
    public class EmailServerContext
    {
        private IEmailProvider? _provider;

        public EmailServerContext(string provedor)
        {
            switch (provedor)
            {
                case "gmail":
                    _provider = new GmailProvider();
                    break;
                case "office365":
                    _provider = new Office365Provider();
                    break;
            }
        }

        public bool EnviarEmail(Email email)
        {
            return _provider.EnviarEmail(email);
        }

        public List<Email> ReceberEmail()
        {
            return _provider.ReceberEmails();
        }
    }
}
