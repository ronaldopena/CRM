using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class GerarPdfResposta
    {
        public int sucesso { get; set; }
        public string? mensagem { get; set; }
        public string? arquivo { get; set; }
        public string? url { get; set; }
    }
}
