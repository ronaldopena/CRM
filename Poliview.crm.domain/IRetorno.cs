using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class IRetorno
    {
        public int status { get; set; }
        public Boolean sucesso { get; set; }        
        public string? mensagem { get; set; }       
    }
}
