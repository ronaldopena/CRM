using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poliview.crm.domain
{
    public class OcorrenciasAberturaChamado
    {
        public int id { get; set; }
        public int idpai { get; set; }
        public string? descricao { get; set; }
        public int ordem { get; set; }
    }
}
