using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class Retorno
    {
        public string mensagem { get; set; }
        public bool sucesso { get; set; }
        public Object? objeto { get; set; }
    }
}
