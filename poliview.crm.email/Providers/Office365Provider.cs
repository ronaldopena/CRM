using poliview.crm.email.Interfaces;
using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poliview.crm.email.Providers
{
    public class Office365Provider : IEmailProvider
    {
        public bool EnviarEmail(Email email)
        {
            throw new NotImplementedException();
        }

        public List<Email> ReceberEmails()
        {
            throw new NotImplementedException();
        }
    }
}
