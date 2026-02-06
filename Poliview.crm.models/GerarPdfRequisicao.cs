using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.models
{
    public class GerarPdfRequisicao
    {
        public int tiporelatorio { get; set; }
        public string? contrato { get; set; }
        public string? contratooriginal { get; set; }
        public int cobranca { get; set; }
        public int recebimento { get; set; }
        public int ano { get; set; }
        public string? email { get; set; }
        public string? codigoclientesp7 { get; set; }
        public Boolean download { get; set; }
    }
}
