using Poliview.crm.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace poliview.crm.email.Interfaces
{
    public interface IEmailProvider
    {
        bool EnviarEmail(Email email);
        List<Email> ReceberEmails();
    }
}
