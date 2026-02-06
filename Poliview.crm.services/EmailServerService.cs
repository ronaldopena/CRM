using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.services
{
    public interface IEmailServerService
    {
        public Boolean Send();
        public Boolean Receive();
    }
    public class EmailServerService : IEmailServerService
    {
        public bool Receive()
        {
            throw new NotImplementedException();
        }

        public bool Send()
        {
            throw new NotImplementedException();
        }
    }
}
