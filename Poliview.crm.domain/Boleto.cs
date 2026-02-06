using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Boleto
    {
        public string BOLETODTVENC { get; set; }
        public string LINHADIGITAVEL { get; set; }
        public float BOLETOVALOR { get; set; }
        public int RECTO { get; set; }
        public int COBRANCA { get; set; }
        public string CONTRATO { get; set; }
    }
}
