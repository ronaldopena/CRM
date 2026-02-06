using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.services
{
    public interface ITokenService
    {
        string GenerateToken(string emailouCPF);
    }
}
