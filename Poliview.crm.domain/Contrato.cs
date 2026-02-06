using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class Contrato
    {
        public string codigocliente { get; set; }
        public string nome { get; set; }
        public DateTime nascimento { get; set; }
        public string? codigocontrato { get; set; }
        public DateTime assinatura { get; set; }
        public float percentual { get; set; }
        public string? empreendimento { get; set; }
        public string? bloco { get; set; }
        public string? unidade { get; set; }
        public Double saldodevedor { get; set; } = 999999999999;
        public DateTime dataquitacao { get; set; }
        public int distratado { get; set; }
        public int remanejado { get; set; }
        public int cancelado { get; set; }
        public int status { get; set; }
        public string ativo { get; set; }
        
    }
}


// prop.proponente_percentual as percentual, con.ctr_cdg as codigocontrato, con.ctr_dtassinatura as assintatura, con.ctr_remanejado as remanejado, con.ctr_cancelado as cancelado